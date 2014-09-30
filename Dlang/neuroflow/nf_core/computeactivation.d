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

	void computeForward(Object context, ref MLPForwardNode[] nodes);

	void computeBackward(Object context, ref MLPBackwardNode[] nodes, in GradientComputationPhase phase, in size_t internalIterationCount);

	void computeGradientsRTLR();
}