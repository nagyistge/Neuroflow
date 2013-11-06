#include "stdafx.h"
#include "OCLContextImpl.h"
#include "NUtils.h"
#include "OCLError.h"
#include "OCLVectorUtils.h"
#include "OCLIntCtx.h"
#include "OCLDataArrayFactory.h"
#include "OCLComputeGradientDescent.h"
#include "OCLMultilayerPerceptronAdapter.h"
#include "OCLDeviceArrayManagement.h"
#include "OCLComputeActivation.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN; 

namespace NeuroflowN
{
    typedef vector<tuple<string, Context, Device>> clDeviceInfoVecT;

    clDeviceInfoVecT GetAvailableDevices(cl_device_type type)
    {
        clDeviceInfoVecT all;
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

                Context context(type, cprops, nullptr, nullptr, nullptr);

                auto devices = context.getInfo<CL_CONTEXT_DEVICES>();

                for (auto& d : devices)
                {
                    auto v = d.getInfo<CL_DEVICE_OPENCL_C_VERSION>();
                    v.erase(v.begin(), find_if(v.begin(), v.end(), ptr_fun<int, int>(isspace)));
                    v.erase(v.begin() + 1, find_if(v.begin() + 1, v.end(), ptr_fun<int, int>(isspace)));
                    stringstream ss;
                    double oclv;
                    ss << v;
                    ss >> oclv;
                    if (oclv >= 1.2) all.push_back(make_tuple(platformName, context, d));
                }
            }
            catch (...) 
            {
            }
        }

        return move(all);
    }

    string GetDeviceName(const Device& device)
    {
        return trim(device.getInfo<CL_DEVICE_NAME>());
    }

    string CreateDeviceID(const string& platformName, const Device& device)
    {
        string devVersion = trim(device.getInfo<CL_DRIVER_VERSION>());
        stringstream idBuilder;
        idBuilder << platformName.c_str() << " / " << GetDeviceName(device) << " / " << devVersion.c_str();
        return idBuilder.str();
    }
}

OCLContextImpl::OCLContextImpl(const std::string& deviceID, const std::string& version) :
    dataArrayFactory(nullptr),
    vectorUtils(nullptr),
    ctx(nullptr),
    version(version)
{
    try
    {
        cl_device_type type = CL_DEVICE_TYPE_ALL;
        string upper = deviceID;
        transform(upper.begin(), upper.end(), upper.begin(), toupper);
        if (upper == "GPU")
        {
            type = CL_DEVICE_TYPE_GPU;
        }
        else if (upper == "CPU")
        {
            type = CL_DEVICE_TYPE_CPU;
        }

        auto deviceInfos = NeuroflowN::GetAvailableDevices(type);

        if (type == CL_DEVICE_TYPE_ALL)
        {
            // Find exact device by ID:
            for (auto& di : deviceInfos)
            {
                string platformName = get<0>(di);
                auto context = get<1>(di);
                auto device = get<2>(di);
                auto id = CreateDeviceID(platformName, device);

                if (id == deviceID)
                {
                    // Found!
                    Initialize(DeviceInfo(id, GetDeviceName(device), platformName), device);
                    return;
                }
            }

            // Find by partial device id:
            for (auto& di : deviceInfos)
            {
                string platformName = get<0>(di);
                auto context = get<1>(di);
                auto device = get<2>(di);
                auto id = CreateDeviceID(platformName, device);

                if (boost::ifind_first(id, deviceID))
                {
                    // Found!
                    Initialize(DeviceInfo(id, GetDeviceName(device), platformName), device);
                    return;
                }
            }
        }
        else if (deviceInfos.size() > 0)
        {
            auto best = deviceInfos.cbegin();
            unsigned bestCores = 0;
            for (auto current = deviceInfos.cbegin(); current != deviceInfos.cend(); current++)
            {
                string platformName = get<0>(*current);
                auto context = get<1>(*current);
                auto device = get<2>(*current);

                auto cores = device.getInfo<CL_DEVICE_MAX_COMPUTE_UNITS>();
                
                if (cores > bestCores)
                {
                    bestCores = cores;
                    best = current;
                }
            }

            // Found!
            string platformName = get<0>(*best);
            auto device = get<2>(*best);
            Initialize(DeviceInfo(CreateDeviceID(platformName, device), GetDeviceName(device), platformName), device);
            return;
        }

        stringstream error;
        error << "Device '";
        error << string(deviceID.cbegin(), deviceID.cend()).c_str();
        error << "' is not found.";
        throw_logic_error(error.str().c_str());
    }
    catch (logic_error&)
    {
        throw;
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

DataArrayFactory* OCLContextImpl::GetDataArrayFactoryPtr() const
{
    return dataArrayFactory.get();
}

IVectorUtils* OCLContextImpl::GetVectorUtilsPtr() const
{
    return vectorUtils.get();
}

IMultilayerPerceptronAdapter* OCLContextImpl::GetMultilayerPerceptronAdapterPtr() const
{
    return multilayerPerceptronAdapter.get();
}

IDeviceArrayManagement* OCLContextImpl::GetDeviceArrayManagementPtr() const
{
    return deviceArrayManagement.get();
}

void OCLContextImpl::Free()
{
    struct Args
    {
        OCLContextImpl* toDel;
    };

    Args args;
    args.toDel = this;

    try
    {
        ctx->GetQueue().enqueueNativeKernel(
            [](void* userData)
            {
                auto passed = reinterpret_cast<Args*>(userData);
                delete passed->toDel;
            },
            make_pair(&args, sizeof(Args)));
    }
    catch (Error&)
    {
        // Native kernel is not supported:
        using namespace concurrency;
        create_task([=]()
        {
            ctx->GetQueue().finish();
            delete args.toDel;
        });
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void OCLContextImpl::Initialize(const DeviceInfo& deviceInfo, const cl::Device& device)
{
    try
    {
        ctx = make_shared<OCLIntCtx>(device, deviceInfo, version);
        
        this->deviceInfo = deviceInfo;
        vault = make_shared<OCLVault>(ctx);
        dataArrayFactory = make_shared<OCLDataArrayFactory>(ctx);
        vectorUtils = make_shared<OCLVectorUtils>(ctx, vault);
        deviceArrayManagement = make_shared<OCLDeviceArrayManagement>(ctx);
        multilayerPerceptronAdapter = make_shared<OCLMultilayerPerceptronAdapter>(ctx, vault, vectorUtils, deviceArrayManagement);

        ctx->preferredWorkgroupSizeMul = vectorUtils->GetPreferredWorkgroupSizeMul();
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

DeviceInfoVecT OCLContextImpl::GetAvailableDevices()
{
    DeviceInfoVecT all;
    try
    {
        auto deviceInfos = NeuroflowN::GetAvailableDevices(CL_DEVICE_TYPE_ALL);

        for (auto& di : deviceInfos)
        {
            string platformName = get<0>(di);
            auto device = get<2>(di);

            all.push_back(DeviceInfo(CreateDeviceID(platformName, device), GetDeviceName(device), platformName));
        }

        return move(all);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

const DeviceInfo& OCLContextImpl::GetDevice() const
{
    return deviceInfo;
}

const OCLVaultSPtrT& OCLContextImpl::GetVault() const
{
    return vault;
}
