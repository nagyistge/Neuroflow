import devicearray;
import devicearray2;
import activationdescription;
import aliases;
import std.typecons;
import supervisedoutputs;

struct WeightedErrors
{
	DeviceArray errors;

	DeviceArray2 weights;
}

struct MLPBackwardNode
{
	ActivationDescription activation;

	GetDeviceArray[] inputs;
	
	DeviceArray2[] gradients;

	DeviceArray2[] gradientSums;

	DeviceArray biasGradients;

	DeviceArray biasGradientSums;

	GetDeviceArray outputs;

	DeviceArray errors;

	WeightedErrors[] lowerErrors;

	Nullable!SupervisedOutputs netOutputs;

	@property bool hasGradients()
	{
		return biasGradients !is null && gradients !is null && gradients.length;
	}

	@property bool hasGradientSums()
	{
		return biasGradientSums !is null && gradientSums !is null && gradientSums.length;
	}

	@property size_t size()
	{
		if (hasGradients) return biasGradients.size;
		if (hasGradientSums) return biasGradientSums.size;
		return 0;
	}

	@property bool isLast()
	{
		return !netOutputs.isNull;
	}
}
