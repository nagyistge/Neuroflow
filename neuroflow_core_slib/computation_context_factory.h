#pragma once

#include "nfdev.h"
#include "device_info.h"

namespace nf
{
    struct computation_context_factory
    {
        computation_context_factory();

        std::list<device_info> get_available_devices(const wchar_t* typeId) const;
        
        computation_context_ptr create_context(const std::wstring& typeId, const std::wstring& deviceHint = L"", const cc_init_pars* properties = null) const
        {
            return create_context(typeId.c_str(), deviceHint, properties);
        }
        
        computation_context_ptr create_context(const wchar_t* typeId, const std::wstring& deviceHint = L"", const cc_init_pars* properties = null) const;
        void register_type(const wchar_t* typeId, const cc_factory_adapter_ptr& adapter);
        static const computation_context_factory& instance();

    private:
        std::unordered_map<std::wstring, cc_factory_adapter_ptr> adapters;
        static computation_context_factory _singleton;

        inline static std::string create_type_not_found_msg(const std::wstring& typeId);
    };
}
