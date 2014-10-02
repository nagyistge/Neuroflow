import mlp;

struct RTLR
{
	this(MLP mlp)
	{
		assert(mlp);

		_mlp = mlp;
	}

	void initialize()
	{
	}

	void zero()
	{
	}

	private MLP _mlp;
}
