#pragma once

#include "../nfdev.h"

namespace nf
{
    struct cpp_computation_context;
    typedef std::shared_ptr<cpp_computation_context> cpp_computation_context_ptr;

    struct cpp_learning_impl_factory;
    typedef std::shared_ptr<cpp_learning_impl_factory> cpp_learning_impl_factory_ptr;

    struct cpp_device_array;
    typedef std::shared_ptr<cpp_device_array> cpp_device_array_ptr;

    struct cpp_device_array2;
    typedef std::shared_ptr<cpp_device_array2> cpp_device_array2_ptr;

    struct cpp_device_array_pool;
    typedef std::shared_ptr<cpp_device_array_pool> cpp_device_array_pool_ptr;

    struct cpp_data_array;
    typedef std::shared_ptr<cpp_data_array> cpp_data_array_ptr;
    typedef std::vector<cpp_device_array_ptr> cpp_device_array_collection_t;

    struct cpp_data_array_factory;
    typedef std::shared_ptr<cpp_data_array_factory> cpp_data_array_factory_ptr;

    struct cpp_device_array_management;
    typedef std::shared_ptr<cpp_device_array_management> cpp_device_array_management_ptr;

    struct cpp_utils;
    typedef std::shared_ptr<cpp_utils> cpp_utils_ptr;

    struct cpp_compute_activation;
    typedef std::shared_ptr<cpp_compute_activation> cpp_compute_activation_ptr;
}
