#include "stdafx.h"
#include "OCLComputationState.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLKernelToExecute* OCLComputationState::GetExec(unsigned index, bool isOutOfOrder)
{
    execs.resize(index + 1);
    auto& current = execs[index];
    if (current != null) return current.get();
    return (current = make_unique<OCLKernelToExecute>(isOutOfOrder)).get();
}
