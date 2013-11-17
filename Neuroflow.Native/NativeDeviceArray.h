#pragma once

#include <assert.h>
#include "IDeviceArray.h"
#include "NativePtr.h"

namespace Neuroflow
{
    ref class NativeDeviceArray : public NativePtr<NeuroflowN::IDeviceArray>, public IDeviceArray
    {
    public:
        NativeDeviceArray(NeuroflowN::IDeviceArray* deviceArray) :
            NativePtr(deviceArray)
        {
            assert(deviceArray != nullptr);
        }

        virtual property DeviceArrayType Type
        {
            DeviceArrayType get()
            {
                return DeviceArrayType::DeviceArray;
            }
        }

        virtual property int Size
        {
            int get() 
            {
                return (int) Ptr->GetSize();
            }
        }
    };
};