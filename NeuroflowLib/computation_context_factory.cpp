#include "stdafx.h"
#include "computation_context_factory.h"
#include "cc_factory_adapter.h"

USING;

computation_context_factory::computation_context_factory()
{
}

std::list<device_info> computation_context_factory::get_available_devices(const wchar_t* typeId)
{
    cc_factory_adapter_ptr adapter;
    auto found = adapters.find(typeId);
    if (found == adapters.cend()) throw_runtime_error(create_type_not_found_msg(typeId));
    return found->second->get_available_devices();
}

computation_context_ptr computation_context_factory::create_context(const wchar_t* typeId, const std::wstring& deviceHint, const boost::optional<boost::property_tree::ptree>& properties)
{
    cc_factory_adapter_ptr adapter;
    auto found = adapters.find(typeId);
    if (found == adapters.cend()) throw_runtime_error(create_type_not_found_msg(typeId));
    return found->second->create_context(deviceHint, properties);
}

std::string computation_context_factory::create_type_not_found_msg(const std::wstring& typeId)
{
    string typeIdStr;
    typeIdStr.assign(typeId.cbegin(), typeId.cend());
    return "Computation context type '" + typeIdStr + "' is not registered.";
}