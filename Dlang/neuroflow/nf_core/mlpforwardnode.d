import activationdescription;
import aliases;
import devicearray2;
import devicearray;

struct WeightedInputs
{
	GetDeviceArray inputs;

	DeviceArray2 weights;
}

struct MLPForwardNode
{
	ActivationDescription activation;

	WeightedInputs[] weightedInputs;
	
	GetDeviceArray outputs;

	DeviceArray biases;

	DeviceArray derivates;

	@property size_t size() nothrow
	{
		return biases.size;
	}
}
