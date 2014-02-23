#pragma once

#include "nfdev.h"
#include "learning_init_behavior.h"

namespace nf
{
    struct randomize_weights_uniform : learning_init_behavior
    {
        randomize_weights_uniform()
        {
        }

        randomize_weights_uniform(float strength) : 
            _strength(strength)
        {
        }

        float strength() const
        {
            return _strength;
        }
        void strength(float value)
        {
            _strength = value;
        }

    private:
        float _strength = 1.0f;
    };

    inline layer_behavior_ptr make_randomize_weights_uniform()
    {
        return std::make_shared<randomize_weights_uniform>();
    }

    inline layer_behavior_ptr make_randomize_weights_uniform(float strength)
    {
        return std::make_shared<randomize_weights_uniform>(strength);
    }
}