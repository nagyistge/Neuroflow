#pragma once

#include "ocl_nfdev.h"
#include "device_info.h"

namespace nf
{
    struct ocl_internal_context : nf_object
    {
        ocl_internal_context(const device_info& deviceInfo, const cl::Device& device);

        const device_info& device_info() const;
        const cl::Context cl_context() const;
        const cl::Device cl_device() const;
        const cl::CommandQueue cl_queue() const;

        bool is_cpu() const;

        idx_t max_compute_units() const;
        idx_t max_work_group_size() const;
        idx_t max_connection_count() const;
        idx_t max_layer_count() const;
        idx_t preferred_workgroup_size_mul() const;
        const cl::NDRange& max_work_item_sizes() const;
        idx_t align_bits() const;

    private:
        cl::CommandQueue queue;
        cl::Device device;
        cl::Context context;
        nf::device_info deviceInfo;
        std::string version;

        bool isCPU;
        idx_t maxWorkGroupSize = 0;
        idx_t maxComputeUnits = 0;
        idx_t preferredWorkgroupSizeMul = 0;
        idx_t alignBits = 0;
        cl::NDRange maxWorkItemSizes;
    };
}