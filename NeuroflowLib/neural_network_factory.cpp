#include "stdafx.h"
#include "neural_network_factory.h"
#include "multilayer_perceptron.h"

USING

neural_network_factory::neural_network_factory(const computation_context_wptr& context) :
weak_contexted(context)
{
}

multilayer_perceptron_ptr neural_network_factory::create_multilayer_perceptron(layers_t& layers, const optional_properties_t& properties)
{
    return shared_ptr<multilayer_perceptron>(new multilayer_perceptron(lock_context(), layers, properties));
}