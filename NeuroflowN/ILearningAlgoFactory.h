#pragma once
#include "Typedefs.h"
#include "NNMetadata.h"
#include "NfObject.h"
#include "ILearningAlgo.h"

namespace NeuroflowN
{
    class ILearningAlgoFactory : public NfObject
    {
    public:
        virtual ILearningAlgo* CreateLearningAlgo(const LearningBehaviorSPtrT& learningBehavior, const TrainingNodeVecT& nodes) = 0;
    };
}