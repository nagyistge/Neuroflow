#pragma once

#include "nfdev.h"
#include "contexted.h"

namespace nf
{
    struct multilayer_perceptron : contexted<computation_context>, virtual nf_object
    {
        friend struct neural_network_factory;

        const boost::property_tree::ptree& properties() const;
        gradient_computation_method gradient_computation_method() const;

    private:
        multilayer_perceptron(const computation_context_ptr& context, const layers_t& layers, const optional_properties_t& properties);

        properties_t _properties;
        nf::gradient_computation_method _gradient_computation_method;
    };
}