#pragma once

#include "NfObject.h"
#include "OCLTypedefs.h"
#include "OCLKernelToExecute.h"
#include <vector>

namespace NeuroflowN
{
    class OCLComputationState : public NfObject
    {
        std::vector<std::unique_ptr<OCLKernelToExecute>> execs;
    public:
        OCLKernelToExecute* GetExec(unsigned index);
    };
}
