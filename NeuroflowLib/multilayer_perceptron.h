#pragma once

#include "nfdev.h"
#include "contexted.h"

namespace nf
{
    struct multilayer_perceptron : contexted<computation_context>, virtual nf_object
    {
        struct layer_info
        {
            layer_info(idx_t index, bool isOnline, bool isOffline, learning_algo_optimization_type optimization_type) : 
                index(index), 
                is_online(isOnline), 
                is_offline(isOffline),
                optimization_type(optimization_type)
            { }

            idx_t index;
            bool is_online, is_offline;
            learning_algo_optimization_type optimization_type;
        };

        friend struct neural_network_factory;

        typedef std::vector<linq::row_numbered<layer_ptr>> ordered_layers_t;

        const boost::property_tree::ptree& properties() const;
        gradient_computation_method gradient_computation_method() const;

    private:
        multilayer_perceptron(const computation_context_ptr& context, layers_t& layers, const optional_properties_t& properties);

        properties_t _properties;
        nf::gradient_computation_method _gradient_computation_method;
        ordered_layers_t _layers;
    };
}