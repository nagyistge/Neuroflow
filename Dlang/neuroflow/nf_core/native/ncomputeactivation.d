import computeactivation;

class NComputeActivation : ComputeActivation
{
	Object createOperationContext() nothrow
	{
		return null;
	}

	void computeForward(Object context, in MLPForwardNode[] nodes) nothrow
	{
		assert(false, "TODO");
	}

	void computeBackward(Object context, in MLPBackwardNode[] nodes, in GradientComputationPhase phase, in size_t internalIterationCount) nothrow
	{
		assert(false, "TODO");
	}

	void computeGradientsRTLR() nothrow
	{
		assert(false, "TODO");
	}
}
