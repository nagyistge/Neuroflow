#pragma once

#include "ILearningAlgo.h"

namespace NeuroflowN
{
    template <typename TRule>
    class LearningAlgo : public ILearningAlgo
    {
    public:
        LearningAlgo(const std::shared_ptr<TRule>& rule, const TrainingNodeVecT& nodes) :
            rule(rule),
            nodes(nodes)
        {
        }

        void Initalize() { }

    protected:
        std::shared_ptr<TRule> rule;
        TrainingNodeVecT nodes;
    };
}