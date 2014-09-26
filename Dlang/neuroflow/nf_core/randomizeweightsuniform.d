import learninginitbehavior;
import std.conv;
import std.exception;

class RandomizeWeightsUniform : LearningInitBehavior
{
	this(float strength)
	{
		_strength = strength;
	}

	override hash_t makeHashCode() const 
	{
		return to!hash_t(_strength * 100000.0);
	}

	override bool arePropsEquals(Object o) const 
	{
		auto rws = cast(RandomizeWeightsUniform)o;
		return rws !is null && rws._strength == _strength;
	}	

	@property float strength() const
	{
		return _strength;
	}	

	private float _strength = 1.0f;
}
