#include "stdafx.h"
#include "OCLComputationState.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLKernelToExecute& OCLComputationState::GetExec(unsigned index, bool noSizeOpt)
{
    if (execs.size() > index) return execs[index];
    execs.resize(index + 1);
    execs[index].noSizeOpt = noSizeOpt;
    return execs[index];
}
