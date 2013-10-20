#include "stdafx.h"
#include "OCLContext.h"
#include "OCLContextImpl.h"
#include "NativeException.h"
#include "NativeDataArrayFactory.h"
#include "NativeMultilayerPerceptronAdapter.h"
#include "NativeVectorUtils.h"
#include "MUtil.h"

using namespace System;
using namespace Neuroflow;

OCLContext::OCLContext(String^ deviceID)
{
    using namespace std;

    try
    {
        oclContext = new NeuroflowN::OCLContextImpl(msclr::interop::marshal_as<string>(deviceID), GetVerison());
        dataArrayFactory = gcnew Data::NativeDataArrayFactory(oclContext->GetDataArrayFactoryPtr());
        multilayerPerceptronAdapter = gcnew NeuralNetworks::NativeMultilayerPerceptronAdapter(oclContext->GetMultilayerPerceptronAdapterPtr());
        vectorUtils = gcnew NativeVectorUtils(oclContext->GetVectorUtilsPtr());
    }
    catch (std::exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

void OCLContext::CleanupNativeResources()
{
    try
    {
        if (oclContext != nullptr)
        {
            oclContext->Free();
            oclContext = nullptr;
        }
    }
    catch (std::exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

array<Neuroflow::Device>^ OCLContext::GetAvailableDevices()
{
    try
    {
        auto devices = NeuroflowN::OCLContextImpl::GetAvailableDevices();
        auto result = gcnew array<Neuroflow::Device>(devices.size());
        int idx = 0;
        for (auto device: devices)
        {
            result[idx++] = Neuroflow::Device(gcnew String(device.ID.c_str()), gcnew String(device.Name.c_str()), gcnew String(device.Platform.c_str()));
        }
        return result;
    }
    catch (std::exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

Neuroflow::Device OCLContext::GetDevice()
{
    try
    {
        auto device = oclContext->GetDevice();
        return Neuroflow::Device(gcnew String(device.ID.c_str()), gcnew String(device.Name.c_str()), gcnew String(device.Platform.c_str()));
    }
    catch (std::exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}
