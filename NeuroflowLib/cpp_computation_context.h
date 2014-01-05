#pragma once

#include "nfdev.h"
#include "computation_context.h"

namespace nf
{
    struct cpp_computation_context : virtual computation_context
    {
        friend struct cpp_cc_factory_adapter;

        const nf::device_info& device_info() const override;
        const properties_t& properties() const override;
        device_array_management_ptr device_array_management() override;
        data_array_factory_ptr data_array_factory() override;
        utils_ptr utils() override;
        compute_activation_ptr compute_activation() override;

    private:
        nf::device_info _deviceInfo;
        properties_t _properties;

        device_array_management_ptr _deviceArrayMan;
        data_array_factory_ptr _dataArrayFactory;
        utils_ptr _utils;
        compute_activation_ptr _computeActivation;

        cpp_computation_context(const std::wstring& deviceHint, const optional_properties_t& properties);
    };
}