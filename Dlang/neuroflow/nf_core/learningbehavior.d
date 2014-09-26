import layerbehavior;

class LearningBehavior : LayerBehavior
{
	override hash_t toHash() const
	{
		try
		{
			return makeHashCode();
		}
		catch (Exception)
		{
			return 0;
		}
	}

	abstract hash_t makeHashCode() const @safe;

	override bool opEquals(Object o) const
	{
		return arePropsEquals(o);
	}

	abstract bool arePropsEquals(Object o) const;
}