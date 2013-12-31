#include "stdafx.h"
#include "ocl_internal_context.h"

USING;

ocl_internal_context::ocl_internal_context(const nf::device_info& deviceInfo, const cl::Device& device) :
context(device),
device(device),
deviceInfo(deviceInfo),
isCPU((device.getInfo<CL_DEVICE_TYPE>() & CL_DEVICE_TYPE_CPU) != 0),
maxComputeUnits(device.getInfo<CL_DEVICE_MAX_COMPUTE_UNITS>()),
maxWorkGroupSize(device.getInfo<CL_DEVICE_MAX_WORK_GROUP_SIZE>()),
maxWorkItemSizes(cl::NullRange),
alignBits(device.getInfo<CL_DEVICE_MEM_BASE_ADDR_ALIGN>())
{
}

const device_info& ocl_internal_context::device_info() const
{
    return deviceInfo;
}

const cl::Context ocl_internal_context::cl_context() const
{
    return context;
}

const cl::Device ocl_internal_context::cl_device() const
{
    return device;
}

const cl::CommandQueue ocl_internal_context::cl_queue() const
{
    return queue;
}

bool ocl_internal_context::is_cpu() const
{
    return isCPU;
}

idx_t ocl_internal_context::max_compute_units() const
{
    return maxComputeUnits;
}

idx_t ocl_internal_context::max_work_group_size() const
{
    return maxWorkGroupSize;
}

idx_t ocl_internal_context::max_connection_count() const
{
    return 4;
}

idx_t ocl_internal_context::max_layer_count() const
{
    return 4;
}

idx_t ocl_internal_context::preferred_workgroup_size_mul() const
{
    assert(preferredWorkgroupSizeMul > 0);
    return preferredWorkgroupSizeMul;
}

const cl::NDRange& ocl_internal_context::max_work_item_sizes() const
{
    return maxWorkItemSizes;
}

idx_t ocl_internal_context::align_bits() const
{
    return alignBits;
}
