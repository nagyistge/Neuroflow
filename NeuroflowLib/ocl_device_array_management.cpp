#include "stdafx.h"
#include "ocl_device_array_management.h"
#include "ocl_device_array.h"
#include "ocl_device_array2.h"
#include "ocl_device_array_pool.h"
#include "ocl_internal_context.h"
#include "ocl_conv.h"

using namespace std;
using namespace nf;
using namespace cl;

ocl_device_array_management::ocl_device_array_management(const ocl_internal_context_ptr& context, const ocl_utils_ptr& utils) :
ocl_contexted(context),
utils(utils)
{
}

device_array_ptr ocl_device_array_management::create_array(bool copyOptimized, idx_t size)
{
    return make_shared<ocl_device_array>(create_buffer(copyOptimized, size * sizeof(float)));
}

device_array2_ptr ocl_device_array_management::create_array2(bool copyOptimized, idx_t rowSize, idx_t colSize)
{
    return make_shared<ocl_device_array2>(create_buffer(copyOptimized, rowSize * colSize * sizeof(float)), rowSize);
}

void ocl_device_array_management::copy(device_array_ptr from, idx_t fromIndex, device_array_ptr to, idx_t toIndex, idx_t size)
{
    try
    {
        context()->cl_queue().enqueueCopyBuffer(
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
    return make_shared<ocl_device_array_pool>(shared_this<ocl_device_array_management>(), utils);
}

cl::Buffer ocl_device_array_management::create_buffer(bool copyOptimized, idx_t sizeInBytes)
{
    try
    {
        auto buffer = Buffer(
            context()->cl_context(),
#if _DEBUG
            0,
#else
            (copyOptimized ? 0 : CL_MEM_HOST_NO_ACCESS),
#endif
            sizeInBytes,
            nullptr);

        context()->cl_queue().enqueueFillBuffer<float>(
            buffer,
            0.0f, //
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