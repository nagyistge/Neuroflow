#include "stdafx.h"
#include "ocl_device_array_management.h"
#include "ocl_device_array.h"
#include "ocl_device_array2.h"
#include "ocl_device_array_pool.h"
#include "ocl_computation_context.h"
#include "ocl_conv.h"

USING;

ocl_device_array_management::ocl_device_array_management(const ocl_computation_context_wptr& context) :
ocl_contexted(context)
{
}

device_array_ptr ocl_device_array_management::create_array(bool copyOptimized, idx_t size)
{
    return make_shared<ocl_device_array>(create_buffer((copyOptimized ? 0 : CL_MEM_HOST_NO_ACCESS), size * sizeof(float)));
}

device_array2_ptr ocl_device_array_management::create_array2(bool copyOptimized, idx_t rowSize, idx_t colSize)
{
    return make_shared<ocl_device_array2>(create_buffer((copyOptimized ? 0 : CL_MEM_HOST_NO_ACCESS), rowSize * colSize * sizeof(float)), rowSize);
}

void ocl_device_array_management::copy(device_array_ptr from, idx_t fromIndex, device_array_ptr to, idx_t toIndex, idx_t size)
{
    auto ctx = lock_context();

    try
    {
        ctx->cl_queue().enqueueCopyBuffer(
            to_ocl(from, false)->buffer(),
            to_ocl(to, false)->buffer(),
            fromIndex * sizeof(float),
            toIndex * sizeof(float),
            size * sizeof(float));
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

device_array_pool_ptr ocl_device_array_management::create_pool()
{
    return make_shared<ocl_device_array_pool>(lock_context());
}

cl::Buffer ocl_device_array_management::create_buffer(cl_mem_flags flags, idx_t sizeInBytes, float fill)
{
    auto ctx = lock_context();

    try
    {
        auto buffer = Buffer(
            ctx->cl_context(),
            flags,
            sizeInBytes,
            nullptr);

        ctx->cl_queue().enqueueFillBuffer<float>(
            buffer,
            fill, //
            0, // offset
            sizeInBytes,
            nullptr,
            nullptr);

        return buffer;
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

cl::Buffer ocl_device_array_management::create_buffer(cl_mem_flags flags, float* from, idx_t sizeInBytes)
{
    auto ctx = lock_context();

    try
    {
        return Buffer(
            ctx->cl_context(),
            flags,
            sizeof(float)* sizeInBytes,
            from);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}