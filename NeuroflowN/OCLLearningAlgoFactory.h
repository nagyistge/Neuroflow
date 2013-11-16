#pragma once

#include "Typedefs.h"
#include "OCLTypedefs.h"
#include "ILearningAlgoFactory.h"

namespace NeuroflowN
{
    class OCLLearningAlgoFactory : public ILearningAlgoFactory
    {
        OCLIntCtxSPtrT ctx;
        OCLVaultSPtrT vault;
        OCLDeviceArrayManagementSPtrT daMan;

        template<typename TRule, typename TAlgo>
        ILearningAlgo* DoCreateLearningAlgo(const LearningBehaviorSPtrT& learningBehavior, const TrainingNodeVecT& nodes)
        {
            return new TAlgo(ctx, vault, daMan, std::dynamic_pointer_cast<TRule>(learningBehavior), nodes);
        }

    public:
        OCLLearningAlgoFactory(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault, const OCLDeviceArrayManagementSPtrT& daMan) :
            ctx(ctx),
            vault(vault),
            daMan(daMan)
        {
        }

        ILearningAlgo* CreateLearningAlgo(const LearningBehaviorSPtrT& learningBehavior, const TrainingNodeVecT& nodes);
    };
}