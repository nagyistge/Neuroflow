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

class MLP
{
    this(ComputationContext ctx, Layer[] layers, MLPInitPars initPars = null)
    {
        assert(ctx);
        assert(layers);
        assert(layers.length);
    }

	@property size_t numberOfWeights()
	{
		return _weights.size + _biases.size;
	}

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