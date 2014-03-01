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
_deviceArrayMan(make_shared<cpp_device_array_management>()),
_dataArrayFactory(make_shared<cpp_data_array_factory>()),
_utils(make_shared<cpp_utils>()),
_computeActivation(make_shared<cpp_compute_activation>()),
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
    return _deviceArrayMan;
}

data_array_factory_ptr cpp_computation_context::data_array_factory()
{
    return _dataArrayFactory;
}

utils_ptr cpp_computation_context::utils()
{
    return _utils;
}

compute_activation_ptr cpp_computation_context::compute_activation()
{
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
        auto thisCtx = shared_this<cpp_computation_context>();
        _learningImplFactory = make_shared<nf::cpp_learning_impl_factory>(thisCtx);
    }
    return _learningImplFactory;
}