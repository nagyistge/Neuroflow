#pragma once

#include "LearningAlgo.h"
#include "OCLTypedefs.h"
#include "OCLComputeGradientDescent.h"

namespace NeuroflowN
{
    struct GradientDescentLearningRule;

    class OCLGradientDescentLearningAlgo : public LearningAlgo<GradientDescentLearningRule>
    {
        OCLIntCtxSPtrT ctx;
        codeVecT gdOnlineCode;
        std::vector<std::function<void(int)>> gdOfflineCode;
        OCLKernelToExecuteVecT gdKernelExecs;
        OCLComputeGradientDescent compute;

    public:
        OCLGradientDescentLearningAlgo(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault, const std::shared_ptr<GradientDescentLearningRule>& rule, const TrainingNodeVecT& nodes);

        LearningAlgoIterationType GetIterationTypes();

        void Run(int iterationCount, IDeviceArray* error);
    };
}