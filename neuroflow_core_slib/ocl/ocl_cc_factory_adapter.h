#pragma once

#include "ocl_nfdev.h"
#include "../cc_factory_adapter.h"

namespace nf
{
    struct ocl_cc_factory_adapter : virtual cc_factory_adapter
    {
        std::list<device_info> get_available_devices() override;
        computation_context_ptr create_context(const std::wstring& deviceHint, const cc_init_pars* properties) override;
    };
};
