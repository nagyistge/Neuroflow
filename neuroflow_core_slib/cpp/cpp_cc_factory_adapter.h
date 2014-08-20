#pragma once

#include "cpp_nfdev.h"
#include "../cc_factory_adapter.h"

namespace nf
{
    struct cpp_cc_factory_adapter : virtual cc_factory_adapter
    {
        std::list<device_info> get_available_devices() override;
        computation_context_ptr create_context(const std::wstring& deviceHint, const cc_init_pars* properties) override;
        static const device_info& only_device();

    private:
        static device_info onlyDevice;
    };
}
