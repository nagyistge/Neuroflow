#include "stdafx.h"
#include "multilayer_perceptron.h"

USING;

multilayer_perceptron::multilayer_perceptron(const computation_context_ptr& context, const optional_properties_t& properties) :
contexted(context)
{
}

const properties_t multilayer_perceptron::properties() const
{
    return _properties;
}