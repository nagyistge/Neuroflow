#pragma once

#include "Typedefs.h"
#include <assert.h>

namespace NeuroflowN
{
#pragma region Layer Behaviors

    enum class WeightUpdateMode
    {
        Online,
        Offline
    };

    enum class LearningAlgoIterationType
    {
        SupervisedOnline = 1 << 1,
        SupervisedOffline = 1 << 2
    };

    struct LayerBehavior
    {
        virtual ~LayerBehavior() { }

        virtual bool operator==(const LayerBehavior& other) const
        {
            return false;
        }
    };

    struct LearningBehavior : public LayerBehavior
    {
        int GroupID;

        LearningBehavior(int groupID) :
            GroupID(groupID)
        {
        }
    };

    struct SupervisedLearningRule : public LearningBehavior
    {
        virtual NeuroflowN::WeightUpdateMode GetWeightUpdateMode() const = 0;

        SupervisedLearningRule(int groupID) :
            LearningBehavior(groupID)
        {
        }
    };

    struct GradientDescentLearningRule : public SupervisedLearningRule
    {
        float LearningRate;
        float Momentum;
        bool Smoothing;
        NeuroflowN::WeightUpdateMode WeightUpdateMode;

        GradientDescentLearningRule(int groupID, float learningRate, float momentum, bool smoothing, NeuroflowN::WeightUpdateMode weightUpdateMode) :
            SupervisedLearningRule(groupID),
            LearningRate(learningRate),
            Momentum(momentum),
            Smoothing(smoothing),
            WeightUpdateMode(weightUpdateMode)
        {
        }

        NeuroflowN::WeightUpdateMode GetWeightUpdateMode() const
        {
            return WeightUpdateMode;
        }
    };

    struct LayerLearningInitializationBehavior : public LayerBehavior
    {
    };

    struct UniformRandomizeWeights : public LayerLearningInitializationBehavior
    {
        float Strength;

        UniformRandomizeWeights(float strength) :
            Strength(strength)
        {
        }
    };

    struct TrainingNode
    {
        TrainingNode(const DeviceArrayVecT& weights, const std::shared_ptr<DeviceArrayVecT>& gradients, const std::shared_ptr<DeviceArrayVecT>& gradientSums) :
            Weights(weights),
            Gradients(gradients),
            GradientSums(gradientSums)
        {
            assert(weights.size() > 0);
        }

        DeviceArrayVecT Weights;

        std::shared_ptr<DeviceArrayVecT> Gradients;

        std::shared_ptr<DeviceArrayVecT> GradientSums;
    };

    typedef std::vector<TrainingNode> TrainingNodeVecT;

#pragma endregion

#pragma region Layer Descriptions

    enum class ActivationFunction
    {
        Sigmoid,
        Linear
    };
    
#pragma endregion

}