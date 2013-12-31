#include "stdafx.h"
#include "ocl_device_array_pool.h"
#include "ocl_device_array.h"
#include "ocl_device_array2.h"
#include "ocl_device_array_management.h"
#include "ocl_internal_context.h"
#include "ocl_utils.h"

USING;

ocl_device_array_pool::ocl_device_array_pool(const ocl_device_array_management_ptr& deviceArrayMan, const ocl_utils_ptr& utils) :
deviceArrayMan(deviceArrayMan),
utils(utils)
{
    assert(deviceArrayMan != null);
    assert(utils != null);
}

bool ocl_device_array_pool::is_allocated() const
{
    return buffer() != null;
}

device_array_ptr ocl_device_array_pool::create_array(idx_t size)
{
    return make_shared<ocl_device_array>(shared_this<ocl_device_array_pool>(), reserve(size), size);
}

device_array2_ptr ocl_device_array_pool::create_array2(idx_t rowSize, idx_t colSize)
{
    return make_shared<ocl_device_array2>(shared_this<ocl_device_array_pool>(), reserve(rowSize * colSize), rowSize, colSize);
}

void ocl_device_array_pool::allocate()
{
    if (endIndex == 0) throw_logic_error("There is no allocated memory in the pool.");
    if (!is_allocated()) buffer = deviceArrayMan->create_buffer(CL_MEM_HOST_NO_ACCESS, endIndex);
}

void ocl_device_array_pool::zero()
{
    if (!is_allocated()) throw_logic_error("Cannot zero out an unallocated pool.");
    idx_t fsize = endIndex / sizeof(float);
    assert(endIndex % sizeof(float) == 0);
    utils->zero(buffer, fsize);
}

idx_t ocl_device_array_pool::reserve(idx_t size)
{
    if (is_allocated()) throw_logic_error("Cannot reserve memory in an already allocated pool.");
    if (endIndex != 0)
    {
        idx_t align = deviceArrayMan->context()->align_bits(); // bits
        align /= 8; // bytes
        while (endIndex % align != 0) endIndex++;
    }
    idx_t beginIndex = endIndex;
    endIndex += size * sizeof(float);
    return beginIndex;
}

cl::Buffer ocl_device_array_pool::create_sub_buffer(idx_t beginOffset, idx_t size)
{
    allocate();
    cl_buffer_region r;
    r.origin = beginOffset;
    r.size = size * sizeof(float);
    return buffer.createSubBuffer(CL_MEM_HOST_NO_ACCESS, CL_BUFFER_CREATE_TYPE_REGION, &r);
}