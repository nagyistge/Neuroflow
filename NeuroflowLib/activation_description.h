#pragma once

#include "nfdev.h"
#include "layer_description.h"

namespace nf
{
    struct activation_description : layer_description
    {
        activation_description()
        {
        }

        activation_description(activation_function function, float alpha) :
            _function(function),
            _alpha(alpha)
        {
        }

        activation_function function() const
        {
            return _function;
        }

        void function(activation_function function)
        {
            _function = function;
        }

        float alpha() const
        {
            return _alpha;
        }

        void alpha(float alpha)
        {
            _alpha = alpha;
        }

    private:
        activation_function _function = activation_function::sigmoid;
        float _alpha = 1.7f;
    };

    inline layer_description_ptr make_activation_description()
    {
        return std::make_shared<activation_description>();
    }

    inline layer_description_ptr make_activation_description(activation_function function, float alpha)
    {
        return std::make_shared<activation_description>(function, alpha);
    }
}