#include "stdafx.h"
#include "ocl_data_array_factory.h"
#include "ocl_data_array.h"
#include "ocl_device_array_management.h"
#include "ocl_computation_context.h"

USING;

ocl_data_array_factory::ocl_data_array_factory(const ocl_computation_context_wptr& context) :
ocl_contexted(context)
{
}

data_array_ptr ocl_data_array_factory::create(idx_t size, float fill)
{
    verify_arg(size > 0, "size");

    auto ctx = lock_context();

    return make_shared<ocl_data_array>(ctx, ctx->ocl_device_array_management()->create_buffer(0, size * sizeof(float), fill), false);
}

data_array_ptr ocl_data_array_factory::create(float* values, idx_t beginPos, idx_t size)
{
    verify_arg(values != null, "values");
    verify_arg(beginPos >= 0, "beginPos");
    verify_arg(size > 0, "size");

    auto ctx = lock_context();

    return make_shared<ocl_data_array>(ctx, ctx->ocl_device_array_management()->create_buffer(CL_MEM_COPY_HOST_PTR, values + beginPos, size * sizeof(float)), false);
}

data_array_ptr ocl_data_array_factory::create_const(float* values, idx_t beginPos, idx_t size)
{
    verify_arg(values != null, "values");
    verify_arg(beginPos >= 0, "beginPos");
    verify_arg(size > 0, "size");

    auto ctx = lock_context();

    return make_shared<ocl_data_array>(ctx, ctx->ocl_device_array_management()->create_buffer(CL_MEM_COPY_HOST_PTR | CL_MEM_HOST_NO_ACCESS | CL_MEM_READ_ONLY, values + beginPos, size * sizeof(float)), true);
}