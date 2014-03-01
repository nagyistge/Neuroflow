#include "stdafx.h"
#include "ocl_cc_factory_adapter.h"
#include "ocl_computation_context.h"

USING

std::list<device_info> ocl_cc_factory_adapter::get_available_devices()
{
    try
    {
        list<device_info> result;
        auto devices = ocl_computation_context::get_available_devices();
        transform(devices.cbegin(), devices.cend(), back_inserter(result), [](const pair<device_info, cl::Device>& p) { return p.first; });
        return result;
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

computation_context_ptr ocl_cc_factory_adapter::create_context(const std::wstring& deviceHint, const optional_properties_t& properties)
{
    return ocl_computation_context_ptr(new ocl_computation_context(deviceHint, properties));
}