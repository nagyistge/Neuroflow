import layer;
import mlp;
import std.exception;
import computationcontext;
import computeactivation;
import mlpinitpars;
import devicearray;
import devicearray2;
import devicearraypool;
import devicearraygroup;
import devicearray2group;
import devicearraymanagement;
import std.typecons;
import std.range;
import std.algorithm;
import std.array;
import layerordercomparer;
import dataarray;
import learningimplfactory;
import supervisedlearningbehavior;
import layerbehavior;
import std.conv;
import activationdescription;
import supervisedoutputs;

class MLP
{
	struct LayerInfo
	{
		size_t index;

		bool isOnline, isOffline;

		SupervisedOptimizationType optimizationType;
	}

    this(ComputationContext ctx, Layer[] layers, MLPInitPars initPars = null)
	in
	{
		assert(ctx);
        assert(layers);
        assert(layers.length);
	}
	body
    {
		_ctx = ctx;
		_computeActivation = ctx.computeActivation;
		_daMan = ctx.deviceArrayManagement;
		_learningImplFactory =ctx.learningImplFactory;
		_gradientsPool = _daMan.createPool(false);
		_gradientSumsPool = _daMan.createPool(false);
		_outputs = DeviceArrayGroup(_daMan.createPool(false));
		_netValueDerivates = DeviceArrayGroup(_daMan.createPool(false));
		_biases = DeviceArrayGroup(_daMan.createPool(false));
		_errors = DeviceArrayGroup(_daMan.createPool(false));
		_weights = DeviceArray2Group(_daMan.createPool(false));
		_biasGradients = DeviceArrayGroup(_gradientsPool);
		_biasGradientSums = DeviceArrayGroup(_gradientSumsPool);
		_gradients = DeviceArray2Group(_gradientsPool);
		_gradientSums = DeviceArray2Group(_gradientSumsPool);

		if (initPars !is null)
		{
			_gradientComputationMethod = initPars.gradientComputationMethod;
        }

		// Sort copy layers
		auto copyOfLayers = layers.dup;
		auto comp = LayerOrderComparer();

		copyOfLayers.sort!((a, b) => comp.less(a, b));

		// Index:
		_layers = zip(sequence!"n", copyOfLayers).array;

		// Gather stuff:
		auto infos = 
			_layers
				.map!(l => 
					  tuple(
							l[0], 
							l[1].behaviors[].map!(b => cast(SupervisedLearningBehavior)b).filter!(b => b !is null).takeOne))
				.map!(t => LayerInfo(
									 t[0], 
									 !t[1].empty ? t[1].front.weightUpdateMode == SupervisedWeightUpdateMode.online : false,
									 !t[1].empty ? t[1].front.weightUpdateMode == SupervisedWeightUpdateMode.offline : false,
									 !t[1].empty ? t[1].front.optimizationType : SupervisedOptimizationType.none))
				.map!(li => tuple(li.index, li))
				.assocArray;

		auto infoValues = infos.values;

		_isTrainingEnabled = infos.length > 0;
		_isGradientsCalculated = !infoValues.filter!(v => v.optimizationType == SupervisedOptimizationType.gradientBased).takeOne.empty;
		_calculateGlobalOfflineError = !infoValues.filter!(v => v.optimizationType == SupervisedOptimizationType.global && v.isOffline).takeOne.empty;
		_calculateGlobalOfflineError = !infoValues.filter!(v => v.optimizationType == SupervisedOptimizationType.global && v.isOnline).takeOne.empty;
		_doBackpropagate = 
			_isTrainingEnabled && 
			_isGradientsCalculated && 
			(_gradientComputationMethod == GradientComputationMethod.feedForward || _gradientComputationMethod == GradientComputationMethod.bptt);
		_isRecurrent = !_layers.filter!(l => l[1].hasRecurrentConnections).takeOne.empty;

		_doFFBP = _doBackpropagate && !_isRecurrent && _gradientComputationMethod == GradientComputationMethod.feedForward;
		_doBPTT = _doBackpropagate && _isRecurrent && _gradientComputationMethod == GradientComputationMethod.bptt;
		_doRTLR = _isTrainingEnabled && _isRecurrent && _gradientComputationMethod == GradientComputationMethod.rtlr;

		enforce(!((_isRecurrent && _isGradientsCalculated && !(_doBPTT || _doRTLR))), "Recurrent Multilayer Perceptron cannot be trained by Feed Forward gradient computation algorithms.");

		createStructure(infos);
		createCompute();
		createTraining(infos);
    }

	private ComputationContext _ctx;

	private Tuple!(size_t, Layer)[] _layers;

	private void delegate() _computeFunc;

	private void delegate(GradientComputationPhase, size_t) _trainFunc;

	private DeviceArrayManagement _daMan;

	private ComputeActivation _computeActivation;

	private LearningImplFactory _learningImplFactory;

    private GradientComputationMethod _gradientComputationMethod = GradientComputationMethod.feedForward;

	// Behavior flags
	private bool _isTrainingEnabled, _isGradientsCalculated, _calculateGlobalOfflineError, _calculateGlobalOnlineError, _doBackpropagate, _isRecurrent, _doFFBP, _doBPTT, _doRTLR;

	private DeviceArray _netInputs;

	private DeviceArray _netOutputs;

	private DeviceArray _netDesiredOutputs;

	private DeviceArray _globalOfflineErrors;

	private DeviceArray _globalOnlineErrors;

	private DeviceArrayPool _gradientsPool;

	private DeviceArrayPool _gradientSumsPool;

	private DeviceArrayGroup _outputs;

	private DeviceArrayGroup _netValueDerivates;

	private DeviceArrayGroup _biases;

	private DeviceArrayGroup _errors;

	private DeviceArray2Group _weights;

	private DeviceArrayGroup _biasGradients;

	private DeviceArrayGroup _biasGradientSums;

	private DeviceArray2Group _gradients;

	private DeviceArray2Group _gradientSums;

	@property GradientComputationMethod gradientComputationMethod() const
	{
		return _gradientComputationMethod;
	}

	@property size_t inputSize()
	{
		return _layers[0][1].size;
	}

	@property size_t outputSize()
	{
		return _layers[_layers.length - 1][1].size;
	}

	@property size_t numberOfWeights()
	{
		return _weights.size + _biases.size;
	}

	void getWeights(DataArray to)
	{
		enforce(to !is null, "Argument 'to' is null.");

		size_t sIdx = 0;

		foreach (bias; _biases.arrays)
		{
			_daMan.copy(bias, 0, to, sIdx, bias.size);
			sIdx += bias.size();
		}

		foreach (weight; _weights.arrays)
		{
			_daMan.copy(weight, 0, to, sIdx, weight.size);
			sIdx += weight.size;
		}
	}

	void setWeights(DataArray from)
	{
		enforce(from !is null, "Argument 'from' is null.");

		size_t sIdx = 0;

		foreach (bias; _biases.arrays)
		{
			_daMan.copy(from, sIdx, bias, 0, bias.size);
			sIdx += bias.size;
		}

		foreach (weight; _weights.arrays)
		{
			_daMan.copy(from, sIdx, weight, 0, weight.size);
			sIdx += weight.size;
		}
	}

	private void createStructure(in LayerInfo[size_t] infos)
	{
		for (size_t lidx = 1; lidx < _layers.length; lidx++)
		{
			auto learningInfo = &(infos[lidx]);
			bool isOutput = lidx == _layers.length - 1;

			auto layer = _layers[lidx][1];
			size_t layerSize = layer.size;

			// Output:
			if (!isOutput)
			{
				_outputs.add(lidx, layerSize);
			}

			// Net Value Derivates:
			if (_doRTLR)
			{
				_netValueDerivates.add(lidx, layerSize);
			}

			// Bias:
			_biases.add(lidx, layerSize);

			// For gradients:
			if (_doBackpropagate)
			{
				// Errors:
				_errors.add(lidx, layerSize);
			}

			if (learningInfo.isOnline || _doBPTT)
			{
				// Bias Gradients:
				_biasGradients.add(lidx, layerSize);
			}

			if (learningInfo.isOffline)
			{
				// Bias Gradient Sums:
				_biasGradientSums.add(lidx, layerSize);
			}

			foreach (inputConnectedLayer; layer.inputLayers)
			{
				auto key = RowCol(getLayerIndex(inputConnectedLayer), lidx);

				// Weights
				_weights.add(key, inputConnectedLayer.size, layer.size);

				if (learningInfo.isOnline || _doBPTT)
				{
					// Gradients:
					_gradients.add(key, inputConnectedLayer.size, layer.size);
				}

				if (learningInfo.isOffline)
				{
					// Gradient Sums:
					_gradientSums.add(key, inputConnectedLayer.size, layer.size);
				}
			}
		}
	}

	private void createCompute()
	{
		MLPForwardNode[] nodes;
		nodes.length = _layers.length - 1;
		for (size_t lidx = 1; lidx < _layers.length; lidx++)
		{
			auto layer = _layers[lidx][1];
			auto node = &(nodes[lidx - 1]);
			bool isLast = lidx == _layers.length - 1;

			node.weightedInputs = 
				layer.inputLayers.map!(
					(inputConnectedLayer)
					{
						size_t inputIndex = getLayerIndex(inputConnectedLayer);
						auto key = RowCol(inputIndex, lidx);
						return WeightedInputs({ return getNetValues(inputIndex); }, _weights.get(key));
					}).array;

			node.activation = getActivationDesc(lidx);
			node.biases = _biases.get(lidx);
			node.outputs = { return getNetValues(lidx); };
			if (_doRTLR) node.derivates = _netValueDerivates.get(lidx);
		}

		auto cctx = _computeActivation.createOperationContext();

		if (_doRTLR)
		{
			_computeFunc =
			{
				_computeActivation.computeForward(cctx, nodes);
				assert(false, "TODO: _rtlr.computeGradients(_netDesiredOutputs);");
			};
		}
		else 
		{
			_computeFunc = 
			{
				_computeActivation.computeForward(cctx, nodes);
			};
		}
	}

	private void createTraining(in LayerInfo[size_t] infos)
	{
		if (_doBackpropagate)
		{
			MLPBackwardNode[] nodes;
			nodes.length = _layers.length - 1;
			for (size_t lidx = _layers.length - 1, nodeidx = 0; lidx >= 1; lidx--, nodeidx++)
			{
				auto layer = _layers[lidx][1];
				auto node = &(nodes[nodeidx]);
				auto learningInfo = &(infos[lidx]);
				if (!learningInfo.isOffline && !learningInfo.isOnline) continue;

				foreach (inputConnectedLayer; layer.inputLayers)
				{
					size_t inputIndex = getLayerIndex(inputConnectedLayer);
					auto key = RowCol(inputIndex, lidx);
					node.inputs ~= { return getNetValues(inputIndex); };
					if (learningInfo.isOnline || _doBPTT) node.gradients ~= _gradients.get(key);
					if (learningInfo.isOffline) node.gradientSums ~= _gradientSums.get(key);
				}

				if (nodeidx == 0)
				{
					// Last layer
					node.netOutputs = SupervisedOutputs({ return getNetValues(lidx); }, { return _netDesiredOutputs; });
				}
				else
				{
					foreach (outputConnectedLayer; layer.outputLayers)
					{
						size_t outputIndex = getLayerIndex(outputConnectedLayer);
						auto key = RowCol(lidx, outputIndex);
						node.lowerErrors ~= WeightedErrors(_errors.get(outputIndex), _weights.get(key));
					}
					node.outputs = { return getNetValues(lidx); };
				}

				node.activation = getActivationDesc(lidx);
				node.errors = _errors.get(lidx);            
				if (learningInfo.isOnline || _doBPTT) node.biasGradients = _biasGradients.get(lidx);
				if (learningInfo.isOffline) node.biasGradientSums = _biasGradientSums.get(lidx);
			}

			auto cctx = _computeActivation.createOperationContext();
			_trainFunc = (phase, inItIdx)
			{
				_computeActivation.computeBackward(cctx, nodes, phase, inItIdx);
			};
		}
		else if (_doRTLR)
		{
			assert(false, "TODO: _rtlr->initialize(this)");
		}
		if (_calculateGlobalOnlineError || _calculateGlobalOfflineError)
		{
			assert(false, "TODO");
		}

		assert(false, "TODO: create_impls();");
	}

	private size_t getLayerIndex(in Layer layer)
	{
		return _layers.filter!(l => l[1] == layer).takeOne.front[0];
	}

	private DeviceArray getNetValues(size_t layerIndex)
	{
		if (layerIndex == 0)
		{
			return _netInputs;
		}
		else if (layerIndex == _layers.length)
		{
			return _netOutputs;
		}
		else
		{
			return _outputs.get(layerIndex);
		}
	}

	private ActivationDescription getActivationDesc(size_t layerIndex)
	{
		auto layer = _layers[layerIndex];
		auto desc = layer[1].descriptions[]
			.map!(d => cast(ActivationDescription)d)
			.filter!(d => d !is null)
			.takeOne;
		
		enforce(!desc.empty, "Layer '" ~ to!string(layer[0]) ~ "' activation description expected.");

		return desc.front;
	}
}