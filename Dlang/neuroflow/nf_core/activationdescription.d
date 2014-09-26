import layerdescription;

enum ActivationFunction
{
	sigmoid,
	linear
}

class ActivationDescription : LayerDescription
{
	this(ActivationFunction activationFunction, float alpha)
	{
		_activationFunction = activationFunction;
		_alpha = alpha;
	}

	@property ActivationFunction activationFunction() const
	{
		return _activationFunction;
	}

	@property float alpha() const
	{
		return _alpha;
	}

	private ActivationFunction _activationFunction = ActivationFunction.sigmoid;

	private float _alpha = 1.7f;
}
