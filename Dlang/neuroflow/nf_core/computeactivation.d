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

	void computeForward(Object context, MLPForwardNode[] nodes);

	void computeBackward(Object context, MLPBackwardNode[] nodes, in GradientComputationPhase phase, in size_t internalIterationCount);

	void computeGradientsRTLR();
}