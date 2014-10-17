#pragma once
#include "IDeviceArrayPool.h"
#include <assert.h>
#include "NativePtr.h"
namespace Neuroflow
{
    ref class NativeDeviceArrayPool :
        public NativePtr<NeuroflowN::IDeviceArrayPool>, public IDeviceArrayPool
    {
    public:
        NativeDeviceArrayPool(NeuroflowN::IDeviceArrayPool* pool) : NativePtr(pool)
        {
            assert(pool != null);
        }

        virtual property bool IsAllocated
        {
            bool get()
            {
                return Ptr->GetIsAllocated();
            }
        }

        virtual IDeviceArray^ CreateArray(int size);

        virtual IDeviceArray2^ CreateArray2(int rowSize, int colSize);

        virtual void Allocate();

        virtual void Zero();
    };
}