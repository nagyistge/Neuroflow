#pragma once

#include <memory>
#include <linqlike.h>
#include "enum_flags.h"
#include "enum_to_string.h"

namespace nf
{
    static const wchar_t* cpp_context = L"cpp";
    static const wchar_t* ocl_context = L"ocl";

    typedef ::size_t idx_t;

    typedef std::shared_ptr<nf_object> nf_object_ptr;

    struct computation_context;
    typedef std::shared_ptr<computation_context> computation_context_ptr;
    typedef std::weak_ptr<computation_context> computation_context_wptr;

    struct cc_factory_adapter;
    typedef std::shared_ptr<cc_factory_adapter> cc_factory_adapter_ptr;

    struct device_array;
    typedef std::shared_ptr<device_array> device_array_ptr;

    struct device_array2;
    typedef std::shared_ptr<device_array2> device_array2_ptr;

    struct device_array_pool;
    typedef std::shared_ptr<device_array_pool> device_array_pool_ptr;

    struct device_array_management;
    typedef std::shared_ptr<device_array_management> device_array_management_ptr;

    struct data_array;
    typedef std::shared_ptr<data_array> data_array_ptr;

    struct data_array_factory;
    typedef std::shared_ptr<data_array_factory> data_array_factory_ptr;

    struct utils;
    typedef std::shared_ptr<utils> utils_ptr;

    typedef boost::property_tree::ptree properties_t;
    typedef boost::optional<properties_t> optional_properties_t;

    struct compute_activation;
    typedef std::shared_ptr<compute_activation> compute_activation_ptr;

    struct neural_network_factory;
    typedef std::shared_ptr<neural_network_factory> neural_network_factory_ptr;

    struct multilayer_perceptron;
    typedef std::shared_ptr<multilayer_perceptron> multilayer_perceptron_ptr;

    struct layer;
    typedef std::shared_ptr<layer> layer_ptr;
    typedef std::function<bool(const layer_ptr&)> layer_visitor_func;

    struct layer_behavior;
    typedef std::shared_ptr<layer_behavior> layer_behavior_ptr;
    typedef std::list<layer_behavior_ptr> layer_behavior_coll;

    struct supervised_learning_behavior;
    typedef std::shared_ptr<supervised_learning_behavior> supervised_learning_behavior_ptr;

    struct layer_description;
    typedef std::shared_ptr<layer_description> layer_description_ptr;
    typedef std::list<layer_description_ptr> layer_description_coll;

    struct supervised_batch;
    struct supervised_sample;
    struct supervised_sample_entry;

    ENUM_STRINGS(weight_update_mode, "offline", "online")
    enum class weight_update_mode
    {
        offline,
        online
    };

    ENUM_STRINGS(learning_algo_optimization_type, "gradient_based", "global")
    enum class learning_algo_optimization_type
    {
        gradient_based, 
        global
    };

    ENUM_STRINGS(gradient_computation_method, "none", "feed_forward", "bptt", "rtlr")
    enum class gradient_computation_method
    {
        none,
        feed_forward,
        bptt,
        rtlr
    };

    ENUM_FLAGS(flow_direction)
    enum class flow_direction
    {
        none = 0,
        one_way = 1 << 0,
        two_way = 1 << 2,
        one_way_to_source = 1 << 3,
        all = one_way | two_way | one_way_to_source
    };

    typedef std::pair<flow_direction, layer_ptr> other_layer_t;
    typedef linqlike::enumerable<layer_ptr> layers_t;
}