#include "stdafx.h"
#include "neural_network_factory.h"
#include "multilayer_perceptron.h"
#include "mlp_init_pars.h"

USING

neural_network_factory::neural_network_factory(const computation_context_wptr& context) :
weak_contexted(context)
{
}

multilayer_perceptron_ptr neural_network_factory::create_multilayer_perceptron(layers_t& layers, const mlp_init_pars* properties)
{
    if (properties != null)
    {
        return shared_ptr<multilayer_perceptron>(new multilayer_perceptron(lock_context(), layers, properties));
    }
    else
    {
        boost::scoped_ptr<mlp_init_pars> p(new mlp_init_pars());
        return shared_ptr<multilayer_perceptron>(new multilayer_perceptron(lock_context(), layers, p.get()));
    }
}