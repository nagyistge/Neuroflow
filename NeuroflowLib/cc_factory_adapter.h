#pragma once

#include "nf.h"
#include "device_info.h"

namespace nf
{
    struct cc_factory_adapter : virtual nf_object
    {
        virtual std::list<device_info> get_available_devices() = 0;
        virtual computation_context_ptr create_context(const std::wstring& deviceHint, const boost::optional<boost::property_tree::ptree>& properties) = 0;
    };
}