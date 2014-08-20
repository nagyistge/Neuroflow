#pragma once

#include "nfdev.h"
#include "device_info.h"

namespace nf
{
    struct cc_factory_adapter : virtual nf_object
    {
        virtual std::list<device_info> get_available_devices() = 0;
        virtual computation_context_ptr create_context(const std::wstring& deviceHint, const cc_init_pars* properties) = 0;
    };
}