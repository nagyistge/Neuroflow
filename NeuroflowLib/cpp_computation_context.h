#pragma once

#include "nfdev.h"
#include "computation_context.h"

namespace nf
{
    struct cpp_computation_context : virtual computation_context
    {
        friend struct cpp_cc_factory_adapter;

        const nf::device_info& device_info() const override;
        random_generator& rnd() override;
        device_array_management_ptr device_array_management() override;
        data_array_factory_ptr data_array_factory() override;
        utils_ptr utils() override;
        compute_activation_ptr compute_activation() override;
        learning_impl_factory_ptr learning_impl_factory() override;

    private:
        nf::device_info _deviceInfo;
        random_generator _generator;

        device_array_management_ptr _deviceArrayMan;
        data_array_factory_ptr _dataArrayFactory;
        utils_ptr _utils;
        compute_activation_ptr _computeActivation;
        learning_impl_factory_ptr _learningImplFactory;

        cpp_computation_context(const std::wstring& deviceHint, const cc_init_pars* properties);
    };
}