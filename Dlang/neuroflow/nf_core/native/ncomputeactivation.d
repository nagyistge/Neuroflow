public import computeactivation;
import ncaforward;
import ncabackward;
import ncagradientsrtlr;

class NComputeActivation : ComputeActivation
{
	Object createOperationContext() nothrow
	{
		return null;
	}

	void computeForward(Object context, MLPForwardNode[] nodes)
	{
		ncaForward(nodes);
	}

	void computeBackward(Object context, MLPBackwardNode[] nodes, in GradientComputationPhase phase, in size_t internalIterationCount)
	{
		ncaBackward(nodes, phase, internalIterationCount);
	}

	void computeGradientsRTLR()
	{
		ncaGradientsRTLR();
	}
}
