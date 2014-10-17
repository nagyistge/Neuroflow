#pragma once

#include <assert.h>
#include "IDeviceArray.h"

namespace Neuroflow
{
    ref class NativeDeviceArray : public Neuroflow::IDeviceArray
    {
        NeuroflowN::IDeviceArray* deviceArray;

    public:
        NativeDeviceArray(NeuroflowN::IDeviceArray* deviceArray) :
            deviceArray(deviceArray)
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
                return (int) deviceArray->GetSize();
            }
        }

        property NeuroflowN::IDeviceArray * PDeviceArray
        {
            NeuroflowN::IDeviceArray * get()
            {
                return deviceArray;
            }
        }
    };
};