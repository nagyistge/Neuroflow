#pragma once

#include "Typedefs.h"
#include "OCLTypedefs.h"
#include "ILearningAlgoFactory.h"

namespace NeuroflowN
{
    class OCLLearningAlgoFactory : public ILearningAlgoFactory
    {
        OCLIntCtxSPtrT ctx;

        template<typename TRule, typename TAlgo>
        ILearningAlgo* DoCreateLearningAlgo(const LearningBehaviorSPtrT& learningBehavior, const TrainingNodeVecT& nodes)
        {
            return new TAlgo(ctx, std::dynamic_pointer_cast<TRule>(learningBehavior), nodes);
        }

    public:
        OCLLearningAlgoFactory(const OCLIntCtxSPtrT& ctx) :
            ctx(ctx)
        {
        }

        ILearningAlgo* CreateLearningAlgo(const LearningBehaviorSPtrT& learningBehavior, const TrainingNodeVecT& nodes);
    };
}