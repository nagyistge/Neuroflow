#pragma once
#include "NfObject.h"
namespace NeuroflowN
{
    class IDeviceArrayPool :
        public NfObject
    {
    public:
        virtual bool GetIsAllocated() const = 0;
        virtual IDeviceArray* CreateArray(int size) = 0;
        virtual IDeviceArray2* CreateArray2(int rowSize, int colSize) = 0;
        virtual void Allocate() = 0;
        virtual void Zero() = 0;
    };
}
