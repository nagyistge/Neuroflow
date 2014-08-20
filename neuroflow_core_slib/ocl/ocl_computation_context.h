#pragma once

#include "ocl_nfdev.h"
#include "../computation_context.h"

namespace nf
{
    struct ocl_computation_context : virtual computation_context
    {
        friend struct ocl_cc_factory_adapter;

        const nf::device_info& device_info() const override;
        random_generator& rnd() override;

        device_array_management_ptr device_array_management() override;
        const ocl_device_array_management_ptr& ocl_device_array_management();
        data_array_factory_ptr data_array_factory() override;
        const ocl_data_array_factory_ptr& ocl_data_array_factory();
        utils_ptr utils() override;
        const ocl_utils_ptr& ocl_utils();
        compute_activation_ptr compute_activation() override;
        const ocl_compute_activation_ptr& ocl_compute_activation();
        learning_impl_factory_ptr learning_impl_factory() override;
        const ocl_learning_impl_factory_ptr& ocl_learning_impl_factory();

        const cl::Context& cl_context() const;
        const cl::Device& cl_device() const;
        const cl::CommandQueue& cl_queue() const;

        const ocl_units_ptr& units();
        const ocl_sizes_ptr& sizes();

        bool is_cpu() const;

        idx_t max_compute_units() const;
        idx_t max_work_group_size() const;
        idx_t max_connection_count() const;
        idx_t max_layer_count() const;
        idx_t preferred_workgroup_size_mul();
        const cl::NDRange& max_work_item_sizes() const;
        idx_t align_bits() const;

    private:

        typedef std::list<std::pair<nf::device_info, cl::Device>> cl_device_list_t;

        std::pair<nf::device_info, cl::Device> _currentDevice;
        random_generator _generator;
        cl::CommandQueue _queue;
        cl::Context _context;

        bool _isCPU;
        idx_t _maxWorkGroupSize = 0;
        idx_t _maxComputeUnits = 0;
        idx_t _maxLayerCount = 4;
        idx_t _maxConnectionCount = 4;
        idx_t _preferredWorkgroupSizeMul = 0;
        idx_t _alignBits = 0;
        cl::NDRange _maxWorkItemSizes;

        ocl_device_array_management_ptr _deviceArrayMan;
        ocl_data_array_factory_ptr _dataArrayFactory;
        ocl_utils_ptr _utils;
        ocl_compute_activation_ptr _computeActivation;
        ocl_learning_impl_factory_ptr _learningImplFactory;

        ocl_units_ptr _units;
        ocl_sizes_ptr _sizes;

        ocl_computation_context(const std::wstring& deviceHint, const cc_init_pars* properties);

        static cl_device_list_t get_available_devices(cl_device_type type = CL_DEVICE_TYPE_ALL);
        static std::pair<nf::device_info, cl::Device> find_device(const std::wstring& deviceHint);
        static std::wstring create_device_id(const std::wstring& platformName, const cl::Device& clDevice);
        static std::wstring get_device_name(const cl::Device& clDevice);
        static std::wstring get_device_version(const cl::Device& clDevice);
    };
}
