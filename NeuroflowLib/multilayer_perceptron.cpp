#include "stdafx.h"
#include "multilayer_perceptron.h"

USING;

multilayer_perceptron::multilayer_perceptron(const computation_context_ptr& context, const multilayer_perceptron_props& properties) :
contexted(context),
_properties(properties)
{
}

const multilayer_perceptron_props& multilayer_perceptron::properties() const
{
    return _properties;
}