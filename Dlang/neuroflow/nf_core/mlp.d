import layer;
import mlp;
import std.exception;
import computationcontext;
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

class MLP
{
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
		//_computeActivation(context->compute_activation()),
		_daMan = ctx.deviceArrayManagement;
		//_learningImplFactory(context->learning_impl_factory()),
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
    }

	@property size_t numberOfWeights()
	{
		return _weights.size + _biases.size;
	}

	private ComputationContext _ctx;

	private Tuple!(size_t, Layer)[] _layers;

	private DeviceArrayManagement _daMan;

    private GradientComputationMethod _gradientComputationMethod = GradientComputationMethod.feedForward;

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
}