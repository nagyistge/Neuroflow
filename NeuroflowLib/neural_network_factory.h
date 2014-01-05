#pragma once

#include "nfdev.h"
#include "weak_contexted.h"
#include "multilayer_perceptron_props.h"

namespace nf
{
    struct neural_network_factory : weak_contexted<computation_context>, virtual nf_object
    {
        neural_network_factory(const computation_context_wptr& context);

        multilayer_perceptron_ptr create_multilayer_perceptron();
        multilayer_perceptron_ptr create_multilayer_perceptron(const multilayer_perceptron_props& properties);
    };
}