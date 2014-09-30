import computeactivation;
import ncaforward;

class NComputeActivation : ComputeActivation
{
	Object createOperationContext() nothrow
	{
		return null;
	}

	void computeForward(Object context, ref MLPForwardNode[] nodes)
	{
		ncaForward(context, nodes);
	}

	void computeBackward(Object context, ref MLPBackwardNode[] nodes, in GradientComputationPhase phase, in size_t internalIterationCount)
	{
		assert(false, "TODO");
	}

	void computeGradientsRTLR()
	{
		assert(false, "TODO");
	}
}
