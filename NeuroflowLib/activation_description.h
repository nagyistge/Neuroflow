#pragma once

#include "nfdev.h"
#include "layer_description.h"

namespace nf
{
    struct activation_description : layer_description
    {
        activation_description(activation_function function, float alpha);
        activation_function function() const;
        float alpha() const;

    private:
        activation_function _function;
        float _alpha;
    };
}