#pragma once

#include "nfdev.h"
#include "contexted.h"
#include "multilayer_perceptron_props.h"

namespace nf
{
    struct multilayer_perceptron : contexted<computation_context>, virtual nf_object
    {
        friend struct neural_network_factory;

        const multilayer_perceptron_props& properties() const;

    private:
        multilayer_perceptron(const computation_context_ptr& context, const multilayer_perceptron_props& properties);

        multilayer_perceptron_props _properties;
    };
}