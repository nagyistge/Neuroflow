public import supervisedlearningbehavior;
import std.conv;

class GradientDescentLearning : SupervisedLearningBehavior
{
	this(float learningRate, float momentum, bool smoothing, SupervisedWeightUpdateMode weightUpdateMode)
	{
		_learningRate = learningRate;
		_momentum = momentum;
		_smoothing = smoothing;
		_weightUpdateMode = weightUpdateMode;
	}

	@property float learningRate() const
	{
		return _learningRate;
	}

	@property float momentum() const
	{
		return _momentum;
	}

	@property bool smoothing() const
	{
		return _smoothing;
	}

	override @property SupervisedWeightUpdateMode weightUpdateMode() const 
	{
		return _weightUpdateMode;
	}

	override @property SupervisedOptimizationType optimizationType() const 
	{
		return SupervisedOptimizationType.gradientBased;
	}

	override hash_t makeHashCode() const 
	{
		return to!hash_t(_learningRate * _momentum * (_smoothing ? 100.0 : 1.0) * 100000.0);
	}

	override bool arePropsEquals(Object o) const 
	{
		auto gdl = cast(GradientDescentLearning)o;
		return 
			gdl !is null && 
			_learningRate == gdl._learningRate && 
			_momentum == gdl._momentum && 
			_smoothing == gdl._smoothing;
	}	

	private float _learningRate = 0.01f;
	
	private float _momentum = 0.25f;
	
	private bool _smoothing = false;
	
	SupervisedWeightUpdateMode _weightUpdateMode = SupervisedWeightUpdateMode.online;
}
