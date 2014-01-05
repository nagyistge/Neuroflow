#pragma once

#include "nfdev.h"
#include "contexted.h"

namespace nf
{
    struct multilayer_perceptron : contexted<computation_context>, virtual nf_object
    {
        friend struct neural_network_factory;

        const properties_t properties() const;

    private:
        multilayer_perceptron(const computation_context_ptr& context, const optional_properties_t& properties);

        properties_t _properties;
    };
}