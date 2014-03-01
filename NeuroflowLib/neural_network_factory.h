#pragma once

#include "nfdev.h"
#include "weak_contexted.h"

namespace nf
{
    struct neural_network_factory : weak_contexted<computation_context>, virtual nf_object
    {
        neural_network_factory(const computation_context_wptr& context);

        multilayer_perceptron_ptr create_multilayer_perceptron(layers_t& layers, const mlp_init_pars* properties = null);

        template <typename TColl>
        multilayer_perceptron_ptr create_multilayer_perceptron(const TColl& layerCollection, const mlp_init_pars* properties = null)
        {
            return create_multilayer_perceptron(linq::from(layerCollection), properties);
        }
    };
}