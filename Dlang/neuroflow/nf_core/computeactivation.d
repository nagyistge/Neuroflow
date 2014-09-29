public import mlpforwardnode;
public import mlpbackwardnode;

enum GradientComputationPhase
{
	ff,
	bpttPhase1,
	bpttPhase2
}

interface ComputeActivation
{
	Object createOperationContext() nothrow;

	void computeForward(Object context, in MLPForwardNode[] nodes) nothrow;

	void computeBackward(Object context, in MLPBackwardNode[] nodes, in GradientComputationPhase phase, in size_t internalIterationCount) nothrow;

	void computeGradientsRTLR() nothrow;
}