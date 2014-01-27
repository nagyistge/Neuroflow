#include "stdafx.h"
#include "ocl_computation_context.h"
#include "prop_def.h"
#include "ocl_device_array_management.h"
#include "ocl_data_array_factory.h"
#include "ocl_utils.h"
#include "ocl_units.h"
#include "ocl_sizes.h"
#include "ocl_compute_activation.h"

USING
using namespace boost::algorithm;

ocl_computation_context::ocl_computation_context(const std::wstring& deviceHint, const optional_properties_t& properties) :
_currentDevice(find_device(deviceHint)),
_context(_currentDevice.second),
_isCPU((_currentDevice.second.getInfo<CL_DEVICE_TYPE>() & CL_DEVICE_TYPE_CPU) != 0),
_maxComputeUnits(_currentDevice.second.getInfo<CL_DEVICE_MAX_COMPUTE_UNITS>()),
_maxWorkGroupSize(_currentDevice.second.getInfo<CL_DEVICE_MAX_WORK_GROUP_SIZE>()),
_maxWorkItemSizes(cl::NullRange),
_alignBits(_currentDevice.second.getInfo<CL_DEVICE_MEM_BASE_ADDR_ALIGN>())
{
    _queue = CommandQueue(_context, _currentDevice.second);
    prop_def pd(_properties, properties);
    _maxConnectionCount = pd.def<idx_t>(ocl_prop_max_connection_count, idx_t(4), [](idx_t v) { return v >= 1 && v <= 10; });
    _maxLayerCount = pd.def<idx_t>(ocl_prop_max_layer_count, idx_t(4), [](idx_t v) { return v >= 1 && v <= 10; });
}

ocl_computation_context::cl_device_list_t ocl_computation_context::get_available_devices(cl_device_type type)
{
    ocl_computation_context::cl_device_list_t all;
    vector<pair<wstring, Device>> all2;
    vector<Platform> platformList;

    Platform::get(&platformList);

    for (auto& p : platformList)
    {
        try
        {
            string platformName;
            p.getInfo((cl_platform_info)CL_PLATFORM_NAME, &platformName);
            trim(platformName);

            cl_context_properties cprops[3] = { CL_CONTEXT_PLATFORM, (cl_context_properties)(p)(), 0 };

            cl::Context context(type, cprops, nullptr, nullptr, nullptr);

            auto devices = context.getInfo<CL_CONTEXT_DEVICES>();

            for (auto& d : devices)
            {
                auto v = get_device_version(d);
                if (boost::lexical_cast<double>(v) >= 1.2)
                {
                    wstring wpn(platformName.cbegin(), platformName.cend());
                    all.emplace_back(nf::device_info(create_device_id(wpn, d), v, get_device_name(d), wpn), d);
                }
            }
        }
        catch (...)
        {
        }
    }

    return move(all);
}

pair<nf::device_info, cl::Device> ocl_computation_context::find_device(const std::wstring& deviceHint)
{
    try
    {
        cl_device_type type = CL_DEVICE_TYPE_ALL;
        wstring upper = to_upper_copy(deviceHint);
        if (upper == L"GPU")
        {
            type = CL_DEVICE_TYPE_GPU;
        }
        else if (upper == L"CPU")
        {
            type = CL_DEVICE_TYPE_CPU;
        }

        auto all = get_available_devices(type);

        if (type == CL_DEVICE_TYPE_ALL)
        {
            // Find exact device by ID:
            for (auto& dev : all)
            {
                if (dev.first.id() == deviceHint)
                {
                    // Found!
                    return dev;
                }
            }

            // Find by partial device id:
            for (auto& dev : all)
            {
                if (ifind_first(dev.first.id(), deviceHint))
                {
                    // Found!
                    return dev;
                }
            }
        }
        else if (all.size() > 0)
        {
            auto best = all.cbegin();
            unsigned bestCores = 0;
            for (auto current = all.cbegin(); current != all.cend(); current++)
            {
                auto cores = current->second.getInfo<CL_DEVICE_MAX_COMPUTE_UNITS>();

                if (cores > bestCores)
                {
                    bestCores = cores;
                    best = current;
                }
            }

            // Found!
            return *best;
        }

        stringstream error;
        error << "Device '";
        error << string(deviceHint.begin(), deviceHint.end());
        error << "' is not found.";
        throw_runtime_error(error.str().c_str());
    }
    catch (runtime_error&)
    {
        throw;
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

std::wstring ocl_computation_context::create_device_id(const std::wstring& platformName, const cl::Device& clDevice)
{
    string devVersion = clDevice.getInfo<CL_DRIVER_VERSION>();
    trim(devVersion);
    wstringstream idBuilder;
    idBuilder << platformName << L" / " << get_device_name(clDevice) << L" / " << devVersion.c_str();
    return idBuilder.str();
}

std::wstring ocl_computation_context::get_device_name(const cl::Device& clDevice)
{
    string devName = clDevice.getInfo<CL_DEVICE_NAME>();
    trim(devName);
    return wstring(devName.cbegin(), devName.cend());
}

std::wstring ocl_computation_context::get_device_version(const cl::Device& clDevice)
{ 
    auto v = clDevice.getInfo<CL_DEVICE_OPENCL_C_VERSION>();
    v.erase(v.begin(), find_if(v.begin(), v.end(), ptr_fun<int, int>(isspace)));
    v.erase(v.begin() + 1, find_if(v.begin() + 1, v.end(), ptr_fun<int, int>(isspace)));
    trim(v);
    return wstring(v.cbegin(), v.cend());
}

const nf::device_info& ocl_computation_context::device_info() const
{
    return _currentDevice.first;
}

const boost::property_tree::ptree& ocl_computation_context::properties() const
{
    return _properties;
}

const cl::Context& ocl_computation_context::cl_context() const
{
    return _context;
}

const cl::Device& ocl_computation_context::cl_device() const
{
    return _currentDevice.second;
}

const cl::CommandQueue& ocl_computation_context::cl_queue() const
{
    return _queue;
}

bool ocl_computation_context::is_cpu() const
{
    return _isCPU;
}

idx_t ocl_computation_context::max_compute_units() const
{
    return _maxComputeUnits;
}

idx_t ocl_computation_context::max_work_group_size() const
{
    return _maxWorkGroupSize;
}

idx_t ocl_computation_context::max_connection_count() const
{
    return _maxConnectionCount;
}

idx_t ocl_computation_context::max_layer_count() const
{
    return _maxLayerCount;
}

idx_t ocl_computation_context::preferred_workgroup_size_mul()
{
    if (_preferredWorkgroupSizeMul == 0)
    {
        _preferredWorkgroupSizeMul = ocl_utils()->get_preferred_workgroup_size_mul();
    }
    return _preferredWorkgroupSizeMul;
}

const cl::NDRange& ocl_computation_context::max_work_item_sizes() const
{
    return _maxWorkItemSizes;
}

idx_t ocl_computation_context::align_bits() const
{
    return _alignBits;
}

device_array_management_ptr ocl_computation_context::device_array_management()
{
    return static_pointer_cast<nf::device_array_management>(ocl_device_array_management());
}

const ocl_device_array_management_ptr& ocl_computation_context::ocl_device_array_management()
{
    if (!_deviceArrayMan)
    {
        auto _this = shared_this<ocl_computation_context>();
        _deviceArrayMan = make_shared<nf::ocl_device_array_management>(_this);
    }
    return _deviceArrayMan;
}

data_array_factory_ptr ocl_computation_context::data_array_factory()
{
    return static_pointer_cast<nf::data_array_factory>(ocl_data_array_factory());
}

const ocl_data_array_factory_ptr& ocl_computation_context::ocl_data_array_factory()
{
    if (!_dataArrayFactory)
    {
        auto _this = shared_this<ocl_computation_context>();
        _dataArrayFactory = make_shared<nf::ocl_data_array_factory>(_this);
    }
    return _dataArrayFactory;
}

utils_ptr ocl_computation_context::utils()
{
    return static_pointer_cast<nf::utils>(ocl_utils());
}

const ocl_utils_ptr& ocl_computation_context::ocl_utils()
{
    if (!_utils)
    {
        auto _this = shared_this<ocl_computation_context>();
        _utils = make_shared<nf::ocl_utils>(_this);
    }
    return _utils;
}

const ocl_units_ptr& ocl_computation_context::units() 
{
    if (!_units)
    {
        auto _this = shared_this<ocl_computation_context>();
        _units = make_shared<nf::ocl_units>(_this);
    }
    return _units;
}

const ocl_sizes_ptr& ocl_computation_context::sizes()
{
    if (!_sizes)
    {
        auto _this = shared_this<ocl_computation_context>();
        _sizes = make_shared<nf::ocl_sizes>(_this);
    }
    return _sizes;
}

compute_activation_ptr ocl_computation_context::compute_activation()
{
    return static_pointer_cast<nf::compute_activation>(ocl_compute_activation());
}

const ocl_compute_activation_ptr& ocl_computation_context::ocl_compute_activation()
{
    if (!_computeActivation)
    {
        auto _this = shared_this<ocl_computation_context>();
        _computeActivation = make_shared<nf::ocl_compute_activation>(_this);
    }
    return _computeActivation;
}