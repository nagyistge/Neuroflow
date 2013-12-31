#pragma once

#include "nfdev.h"
#include "computation_context.h"

namespace nf
{
    struct cpp_computation_context : virtual computation_context
    {
        friend struct cpp_cc_factory_adapter;

        const nf::device_info& device_info() const override;
        const boost::property_tree::ptree& properties() const override;
        const device_array_management_ptr& device_array_management() override;
        const data_array_factory_ptr& data_array_factory() override;
        const utils_ptr& utils() override;

    private:
        nf::device_info deviceInfo;
        boost::property_tree::ptree _properties;

        device_array_management_ptr deviceArrayMan;
        data_array_factory_ptr dataArrayFactory;
        utils_ptr _utils;

        cpp_computation_context(const std::wstring& deviceHint, const boost::optional<boost::property_tree::ptree>& properties);
    };
}