#pragma once

#include <assert.h>
#include "IDeviceArray2.h"

namespace Neuroflow
{
    ref class NativeDeviceArray2 : public Neuroflow::IDeviceArray2
    {
        NeuroflowN::IDeviceArray2* deviceArray;

    public:
        NativeDeviceArray2(NeuroflowN::IDeviceArray2* deviceArray) :
            deviceArray(deviceArray)
        {
            assert(deviceArray != nullptr);
        }

        virtual property DeviceArrayType Type
        {
            DeviceArrayType get()
            {
                return DeviceArrayType::DeviceArray2;
            }
        }

        virtual property int Size
        {
            int get() 
            {
                return (int) deviceArray->GetSize();
            }
        }

        virtual property int Size1
        {
            int get() 
            {
                return (int) deviceArray->GetSize1();
            }
        }

        virtual property int Size2
        {
            int get() 
            {
                return (int) deviceArray->GetSize2();
            }
        }

        property NeuroflowN::IDeviceArray2 * PDeviceArray2
        {
            NeuroflowN::IDeviceArray2 * get()
            {
                return deviceArray;
            }
        }
    };
};