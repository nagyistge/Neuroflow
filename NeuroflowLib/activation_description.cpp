#include "stdafx.h"
#include "activation_description.h"

USING

activation_description::activation_description(activation_function function, float alpha) :
_function(function),
_alpha(alpha)
{
}

activation_function activation_description::function() const
{
    return _function;
}

float activation_description::alpha() const
{
    return _alpha;
}