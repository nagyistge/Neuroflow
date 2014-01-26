#include "stdafx.h"
#include "multilayer_perceptron.h"
#include "prop_def.h"
#include "layer_order_comparer.h"

USING

multilayer_perceptron::multilayer_perceptron(const computation_context_ptr& context, layers_t& layers, const optional_properties_t& properties) :
    contexted(context)
{
    prop_def pd(_properties, properties);
    _gradient_computation_method = pd.defEnum(prop_gradient_computation_method, gradient_computation_method::feed_forward);

    _layers = layers | sort(layer_order_comparer()) | row_num() | to_vector();
}

const boost::property_tree::ptree& multilayer_perceptron::properties() const
{
    return _properties;
}

nf::gradient_computation_method multilayer_perceptron::gradient_computation_method() const
{
    return _gradient_computation_method;
}