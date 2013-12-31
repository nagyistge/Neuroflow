#include "stdafx.h"
#include "cpp_computation_context.h"
#include "cpp_device_array_management.h"
#include "cpp_data_array_factory.h"
#include "cpp_utils.h"
#include "cpp_cc_factory_adapter.h"

USING;

cpp_computation_context::cpp_computation_context(const std::wstring& deviceHint, const boost::optional<boost::property_tree::ptree>& properties) :
deviceArrayMan(make_shared<cpp_device_array_management>()),
dataArrayFactory(make_shared<cpp_data_array_factory>()),
_utils(make_shared<cpp_utils>()),
deviceInfo(cpp_cc_factory_adapter::only_device())
{
}

const nf::device_info& cpp_computation_context::device_info() const
{
    return deviceInfo;
}

const boost::property_tree::ptree& cpp_computation_context::properties() const
{
    return _properties;
}

const device_array_management_ptr& cpp_computation_context::device_array_management()
{
    return deviceArrayMan;
}

const data_array_factory_ptr& cpp_computation_context::data_array_factory()
{
    return dataArrayFactory;
}

const utils_ptr& cpp_computation_context::utils()
{
    return _utils;
}