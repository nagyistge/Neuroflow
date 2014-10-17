#include "stdafx.h"
#include "computation_context_factory.h"
#include "cc_factory_adapter.h"
#include "cpp_cc_factory_adapter.h"
#include "ocl_cc_factory_adapter.h"
#include "cc_init_pars.h"

USING

computation_context_factory computation_context_factory::_default = computation_context_factory();

computation_context_factory::computation_context_factory()
{
    register_type(cpp_context, make_shared<cpp_cc_factory_adapter>());
    register_type(ocl_context, make_shared<ocl_cc_factory_adapter>());
}

const computation_context_factory& computation_context_factory::default()
{
    return _default;
}

void computation_context_factory::register_type(const wchar_t* typeId, const cc_factory_adapter_ptr& adapter)
{
    adapters.insert(make_pair(wstring(typeId), adapter));
}

std::list<device_info> computation_context_factory::get_available_devices(const wchar_t* typeId) const
{
    cc_factory_adapter_ptr adapter;
    auto found = adapters.find(typeId);
    if (found == adapters.cend()) throw_runtime_error(create_type_not_found_msg(typeId));
    return found->second->get_available_devices();
}

computation_context_ptr computation_context_factory::create_context(const wchar_t* typeId, const std::wstring& deviceHint, const cc_init_pars* properties) const
{
    cc_factory_adapter_ptr adapter;
    auto found = adapters.find(typeId);
    if (found == adapters.cend()) throw_runtime_error(create_type_not_found_msg(typeId));
    if (properties != null)
    {
        return found->second->create_context(deviceHint, properties);
    }
    else
    {
        boost::scoped_ptr<cc_init_pars> p(new cc_init_pars());
        return found->second->create_context(deviceHint, p.get());
    }
}

std::string computation_context_factory::create_type_not_found_msg(const std::wstring& typeId)
{
    string typeIdStr;
    typeIdStr.assign(typeId.cbegin(), typeId.cend());
    return "Computation context type '" + typeIdStr + "' is not registered.";
}