#pragma once

#include "nfdev.h"
#include "device_info.h"

namespace nf
{
    struct computation_context : virtual nf_object
    {
        virtual const nf::device_info& device_info() const = 0;
        virtual const boost::property_tree::ptree& properties() const = 0;
        virtual device_array_management_ptr device_array_management() = 0;
        virtual data_array_factory_ptr data_array_factory() = 0;
        virtual utils_ptr utils() = 0;
    };
}