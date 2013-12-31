#pragma once

#include "nfdev.h"
#include "device_info.h"

namespace nf
{
    struct computation_context_factory
    {
        computation_context_factory();

        std::list<device_info> get_available_devices(const wchar_t* typeId) const;
        computation_context_ptr create_context(const wchar_t* typeId, const std::wstring& deviceHint = L"", const boost::optional<boost::property_tree::ptree>& properties = null) const;
        void register_type(const wchar_t* typeId, const cc_factory_adapter_ptr& adapter);
        static const computation_context_factory& default();

    private:
        std::unordered_map<std::wstring, cc_factory_adapter_ptr> adapters;
        static computation_context_factory _default;

        inline static std::string create_type_not_found_msg(const std::wstring& typeId);
    };
}