#pragma once

#include "cpp_nfdev.h"
#include "../computation_context.h"

namespace nf
{
    struct cpp_computation_context : virtual computation_context
    {
        friend struct cpp_cc_factory_adapter;

        const nf::device_info& device_info() const override;
        random_generator& rnd() override;
        
        device_array_management_ptr device_array_management() override;
        const cpp_device_array_management_ptr& cpp_device_array_management();
        
        data_array_factory_ptr data_array_factory() override;
        const cpp_data_array_factory_ptr& cpp_data_array_factory();
        
        utils_ptr utils() override;
        const cpp_utils_ptr& cpp_utils();
        
        compute_activation_ptr compute_activation() override;
        const cpp_compute_activation_ptr& cpp_compute_activation();

        learning_impl_factory_ptr learning_impl_factory() override;
        const cpp_learning_impl_factory_ptr& cpp_learning_impl_factory();

    private:
        nf::device_info _deviceInfo;
        random_generator _generator;

        cpp_device_array_management_ptr _deviceArrayMan;
        cpp_data_array_factory_ptr _dataArrayFactory;
        cpp_utils_ptr _utils;
        cpp_compute_activation_ptr _computeActivation;
        cpp_learning_impl_factory_ptr _learningImplFactory;

        cpp_computation_context(const std::wstring& deviceHint, const cc_init_pars* properties);
    };
}
