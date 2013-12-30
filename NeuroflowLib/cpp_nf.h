#pragma once

#include "nf.h"

namespace nf
{
    struct cpp_device_array;
    typedef std::shared_ptr<cpp_device_array> cpp_device_array_ptr;

    struct cpp_device_array2;
    typedef std::shared_ptr<cpp_device_array2> cpp_device_array2_ptr;

    struct cpp_device_array_pool;
    typedef std::shared_ptr<cpp_device_array_pool> cpp_device_array_pool_ptr;

    struct cpp_data_array;
    typedef std::shared_ptr<cpp_data_array> cpp_data_array_ptr;

    struct cpp_data_array_factory;
    typedef std::shared_ptr<cpp_data_array_factory> cpp_data_array_factory_ptr;
}