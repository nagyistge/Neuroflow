#pragma once

#include <assert.h>
#include "IDeviceArray2.h"
#include "NativePtr.h"

namespace Neuroflow
{
    ref class NativeDeviceArray2 : public NativePtr<NeuroflowN::IDeviceArray2>, public IDeviceArray2
    {
    public:
        NativeDeviceArray2(NeuroflowN::IDeviceArray2* deviceArray) :
            NativePtr(deviceArray)
        {
            assert(deviceArray != null);
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
                return (int) Ptr->GetSize();
            }
        }

        virtual property int Size1
        {
            int get() 
            {
                return (int)Ptr->GetSize1();
            }
        }

        virtual property int Size2
        {
            int get() 
            {
                return (int)Ptr->GetSize2();
            }
        }
    };
};