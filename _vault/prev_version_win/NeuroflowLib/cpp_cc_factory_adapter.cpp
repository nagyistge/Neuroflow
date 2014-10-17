#include "stdafx.h"
#include "cpp_cc_factory_adapter.h"
#include "cpp_computation_context.h"

USING

device_info cpp_cc_factory_adapter::onlyDevice(L"cpp_st", L"1.0", L"C++ Single Threaded", L"x86/x64");

std::list<device_info> cpp_cc_factory_adapter::get_available_devices()
{
    list<device_info> result;
    result.push_back(onlyDevice);
    return move(result);
}

computation_context_ptr cpp_cc_factory_adapter::create_context(const std::wstring& deviceHint, const cc_init_pars* properties)
{
    return cpp_computation_context_ptr(new cpp_computation_context(deviceHint, properties));
}

const device_info& cpp_cc_factory_adapter::only_device()
{
    return onlyDevice;
}