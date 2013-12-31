#pragma once

#include "nf.h"
#include "device_info.h"

namespace nf
{
    struct computation_context_factory
    {
        computation_context_factory();

        std::list<device_info> get_available_devices(const wchar_t* typeId);
        computation_context_ptr create_context(const wchar_t* typeId, const std::wstring& deviceHint, const boost::optional<boost::property_tree::ptree>& properties);
        void register_type(const wchar_t* typeId, const cc_factory_adapter_ptr& adapter);

    private:
        std::unordered_map<std::wstring, cc_factory_adapter_ptr> adapters;

        inline static std::string create_type_not_found_msg(const std::wstring& typeId);
    };
}