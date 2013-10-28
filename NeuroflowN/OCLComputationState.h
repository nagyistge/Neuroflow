#pragma once

#include "NfObject.h"
#include "OCLTypedefs.h"
#include "OCLKernelToExecute.h"
#include <vector>

namespace NeuroflowN
{
    class OCLComputationState : public NfObject
    {
        std::vector<OCLKernelToExecute> execs;
    public:
        OCLKernelToExecute& GetExec(unsigned index);
    };
}
