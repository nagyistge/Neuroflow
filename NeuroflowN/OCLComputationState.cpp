#include "stdafx.h"
#include "OCLComputationState.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLKernelToExecute& OCLComputationState::GetExec(unsigned index)
{
    execs.resize(index + 1);
    return execs[index];
}
