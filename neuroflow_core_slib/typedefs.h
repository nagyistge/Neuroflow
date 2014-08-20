#pragma once

#include <memory>
#include "enum_flags.h"
#include "enum_to_string.h"

namespace nf
{
    static const wchar_t* cpp_context = L"cpp";
    static const wchar_t* ocl_context = L"ocl";

    ENUM_STRINGS(weight_update_mode, "offline", "online")
    enum class weight_update_mode
    {
        offline,
        online
    };

    ENUM_STRINGS(learning_algo_optimization_type, "none", "gradient_based", "global")
    enum class learning_algo_optimization_type
    {
        none,
        gradient_based,
        global
    };

    ENUM_FLAGS(supervised_learning_iteration_type)
    enum class supervised_learning_iteration_type
    {
        online = 1 << 2,
        offline = 1 << 3
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

    ENUM_STRINGS(activation_function, "sigmoid", "linear")
    enum class activation_function
    {
        sigmoid,
        linear
    };

    ENUM_STRINGS(gradient_computation_formula, "ff", "bptt_phase1", "bptt_phase2")
    enum class gradient_computation_formula
    {
        ff,
        bptt_phase1,
        bptt_phase2
    };

    ENUM_STRINGS(sequence_marker, "begin", "inner", "end")
    enum class sequence_marker
    {
        begin = -1,
        inner = 0,
        end = 1
    };

    typedef ::size_t idx_t;

    struct nf_object;
    typedef std::shared_ptr<nf_object> nf_object_ptr;

    struct cc_init_pars;
    struct mlp_init_pars;

    struct computation_context;
    typedef std::shared_ptr<computation_context> computation_context_ptr;
    typedef std::weak_ptr<computation_context> computation_context_wptr;

    struct cc_factory_adapter;
    typedef std::shared_ptr<cc_factory_adapter> cc_factory_adapter_ptr;

    struct device_array;
    typedef std::shared_ptr<device_array> device_array_ptr;
    typedef std::function<device_array*()> get_device_array_ptr_t;
    typedef std::vector<device_array_ptr> device_array_collection_t;

    struct device_array2;
    typedef std::shared_ptr<device_array2> device_array2_ptr;
    typedef std::vector<device_array2_ptr> device_array2_collection_t;
    typedef std::vector<device_array2_collection_t> device_array2_groups_t;

    struct device_array_pool;
    typedef std::shared_ptr<device_array_pool> device_array_pool_ptr;

    struct device_array_management;
    typedef std::shared_ptr<device_array_management> device_array_management_ptr;

    struct data_array;
    typedef std::shared_ptr<data_array> data_array_ptr;
    typedef std::vector<data_array_ptr> data_array_collection_t;

    struct data_array_factory;
    typedef std::shared_ptr<data_array_factory> data_array_factory_ptr;

    struct utils;
    typedef std::shared_ptr<utils> utils_ptr;

    struct compute_activation;
    typedef std::shared_ptr<compute_activation> compute_activation_ptr;

    struct neural_network_factory;
    typedef std::shared_ptr<neural_network_factory> neural_network_factory_ptr;

    struct multilayer_perceptron;
    typedef std::shared_ptr<multilayer_perceptron> multilayer_perceptron_ptr;

    struct layer;
    typedef std::shared_ptr<layer> layer_ptr;
    typedef std::pair<flow_direction, layer_ptr> other_layer_t;
    typedef std::list<layer_ptr> layer_collection_t;

    struct layer_behavior;
    typedef std::shared_ptr<layer_behavior> layer_behavior_ptr;
    typedef std::list<layer_behavior_ptr> layer_behavior_coll_t;

    struct learning_behavior;
    typedef std::shared_ptr<learning_behavior> learning_behavior_ptr;

    struct learning_init_behavior;
    typedef std::shared_ptr<learning_init_behavior> learning_init_behavior_ptr;

    struct supervised_learning_behavior;
    typedef std::shared_ptr<supervised_learning_behavior> supervised_learning_behavior_ptr;

    struct learning_impl;
    typedef std::shared_ptr<learning_impl> learning_impl_ptr;

    struct initialize_learning;
    typedef std::shared_ptr<initialize_learning> initialize_learning_ptr;

    struct supervised_learning;
    typedef std::shared_ptr<supervised_learning> supervised_learning_ptr;

    struct layer_description;
    typedef std::shared_ptr<layer_description> layer_description_ptr;
    typedef std::list<layer_description_ptr> layer_description_coll_t;
    
    struct activation_description;

    struct device_array_group;
    typedef std::shared_ptr<device_array_group> device_array_group_ptr;

    struct device_array2_group;
    typedef std::shared_ptr<device_array2_group> device_array2_group_ptr;

    struct learning_impl;
    typedef std::shared_ptr<learning_impl> learning_impl_ptr;

    struct learning_impl_factory;
    typedef std::shared_ptr<learning_impl_factory> learning_impl_factory_ptr;

    struct training_node;
    typedef std::vector<training_node> training_node_collection_t;
    typedef std::shared_ptr<training_node_collection_t> training_node_collection_ptr;

    struct rtlr_layer_info;
    typedef std::vector<rtlr_layer_info> rtlr_layer_info_collection_t;
    typedef std::vector<rtlr_layer_info_collection_t> rtlr_layer_info_groups_t;

    struct rtlr_computation_data;

    struct supervised_batch;
    struct supervised_sample;
    struct supervised_sample_entry;

    struct mlp_forward_node;
    struct mlp_backward_node;


}
