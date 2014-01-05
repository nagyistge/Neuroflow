#include "stdafx.h"
#include "computation_context.h"
#include "neural_network_factory.h"

USING;

const neural_network_factory_ptr& computation_context::neural_network_factory()
{
    if (!_neuralNetworkFactory) _neuralNetworkFactory = make_shared<nf::neural_network_factory>(shared_this<computation_context>());
    return _neuralNetworkFactory;
}