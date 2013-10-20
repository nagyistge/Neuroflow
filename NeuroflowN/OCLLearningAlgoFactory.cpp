#include "stdafx.h"
#include "OCLLearningAlgoFactory.h"
#include "OCLGradientDescentLearningAlgo.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

ILearningAlgo* OCLLearningAlgoFactory::CreateLearningAlgo(const LearningBehaviorSPtrT& learningBehavior, const TrainingNodeVecT& nodes)
{
    auto gd = dynamic_pointer_cast<GradientDescentLearningRule>(learningBehavior);
    if (gd != nullptr)
    {
        return DoCreateLearningAlgo<GradientDescentLearningRule, OCLGradientDescentLearningAlgo>(learningBehavior, nodes);
    }

    return nullptr;
}