#pragma once

#include "nfdev.h"
#include "supervised_learning_behavior.h"

namespace nf
{
    struct gradient_descent_learning_rule : supervised_learning_behavior
    {
        gradient_descent_learning_rule()
        {
        }

        gradient_descent_learning_rule(float learningRate, float momentum, bool smoothing, nf::weight_update_mode weightUpdateMode) :
            _learningRate(learningRate),
            _momentum(momentum),
            _smoothing(smoothing),
            _weightUpdateMode(weightUpdateMode)
        {
        }

        float learning_rate() const
        {
            return _learningRate;
        }
        void learning_rate(float value)
        {
            _learningRate = value;
        }

        float momentum() const
        {
            return _momentum;
        }
        void momentum(float value)
        {
            _momentum = value;
        }

        bool smoothing() const
        {
            return _smoothing;
        }
        void smoothing(bool value)
        {
            _smoothing = value;
        }

        nf::weight_update_mode weight_update_mode() const override
        {
            return _weightUpdateMode;
        }
        void weight_update_mode(nf::weight_update_mode value)
        {
            _weightUpdateMode = value;
        }

        bool props_equals(const layer_behavior* other) const override;
        ::size_t get_hash_code() const override;
        learning_algo_optimization_type optimization_type() const override;

    private:
        float _learningRate = 0.01f;
        float _momentum = 0.25f;
        bool _smoothing = false;
        nf::weight_update_mode _weightUpdateMode = weight_update_mode::online;
    };

    inline layer_behavior_ptr make_gradient_descent_learning_rule()
    {
        return std::make_shared<gradient_descent_learning_rule>();
    }

    inline layer_behavior_ptr make_gradient_descent_learning_rule(float learningRate, float momentum, bool smoothing, nf::weight_update_mode weightUpdateMode)
    {
        return std::make_shared<gradient_descent_learning_rule>(learningRate, momentum, smoothing, weightUpdateMode);
    }
}