#include "stdafx.h"
#include "cpp_computation_context.h"
#include "cpp_device_array_management.h"
#include "cpp_data_array_factory.h"
#include "cpp_utils.h"
#include "cpp_cc_factory_adapter.h"
#include "cpp_compute_activation.h"
#include "cpp_learning_impl_factory.h"
#include "nf_helpers.h"
#include "cc_init_pars.h"

USING

cpp_computation_context::cpp_computation_context(const std::wstring& deviceHint, const cc_init_pars* properties) :
_deviceInfo(cpp_cc_factory_adapter::only_device()),
_generator(properties->random_seed)
{
}

const nf::device_info& cpp_computation_context::device_info() const
{
    return _deviceInfo;
}

random_generator& cpp_computation_context::rnd() 
{
    return _generator;
}

device_array_management_ptr cpp_computation_context::device_array_management()
{
    return cpp_device_array_management();
}

const cpp_device_array_management_ptr& cpp_computation_context::cpp_device_array_management()
{
    if (_deviceArrayMan == null)
    {
        _deviceArrayMan = make_shared<nf::cpp_device_array_management>();
    }
    return _deviceArrayMan;
}

data_array_factory_ptr cpp_computation_context::data_array_factory()
{
    return cpp_data_array_factory();
}

const cpp_data_array_factory_ptr& cpp_computation_context::cpp_data_array_factory()
{
    if (_dataArrayFactory == null)
    {
        _dataArrayFactory = make_shared<nf::cpp_data_array_factory>();
    }
    return _dataArrayFactory;
}

utils_ptr cpp_computation_context::utils()
{
    return cpp_utils();
}

const cpp_utils_ptr& cpp_computation_context::cpp_utils()
{
    if (_utils == null)
    {
        _utils = make_shared<nf::cpp_utils>();
    }
    return _utils;
}

compute_activation_ptr cpp_computation_context::compute_activation()
{
    return cpp_compute_activation();
}

const cpp_compute_activation_ptr& cpp_computation_context::cpp_compute_activation()
{
    if (_computeActivation == null)
    {
        _computeActivation = make_shared<nf::cpp_compute_activation>();
    }
    return _computeActivation;
}

learning_impl_factory_ptr cpp_computation_context::learning_impl_factory()
{
    return cpp_learning_impl_factory();
}

const cpp_learning_impl_factory_ptr& cpp_computation_context::cpp_learning_impl_factory()
{
    if (!_learningImplFactory)
    {
        _learningImplFactory = make_shared<nf::cpp_learning_impl_factory>(shared_this<cpp_computation_context>());
    }
    return _learningImplFactory;
}