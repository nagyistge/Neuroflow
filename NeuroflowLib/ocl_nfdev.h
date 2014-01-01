#pragma once

#include "nfdev.h"
#include "ocl.h"
#include "ocl_error.h"
#include "ocl_cc_props.h"

namespace nf
{
    struct ocl_computation_context;
    typedef std::shared_ptr<ocl_computation_context> ocl_computation_context_ptr;
    typedef std::weak_ptr<ocl_computation_context> ocl_computation_context_wptr;

    struct ocl_device_array;
    typedef std::shared_ptr<ocl_device_array> ocl_device_array_ptr;

    struct ocl_device_array2;
    typedef std::shared_ptr<ocl_device_array2> ocl_device_array2_ptr;

    struct ocl_device_array_pool;
    typedef std::shared_ptr<ocl_device_array_pool> ocl_device_array_pool_ptr;

    struct ocl_device_array_management;
    typedef std::shared_ptr<ocl_device_array_management> ocl_device_array_management_ptr;

    struct ocl_utils;
    typedef std::shared_ptr<ocl_utils> ocl_utils_ptr;

    struct ocl_data_array;
    typedef std::shared_ptr<ocl_data_array> ocl_data_array_ptr;

    struct ocl_data_array_factory;
    typedef std::shared_ptr<ocl_data_array_factory> ocl_data_array_factory_ptr;

    struct ocl_program;
    typedef std::shared_ptr<ocl_program> ocl_program_ptr;

    struct ocl_program_unit;
    typedef std::shared_ptr<ocl_program_unit> ocl_program_unit_ptr;
}