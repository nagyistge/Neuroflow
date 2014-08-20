#pragma once

#include "Typedefs.h"
#include "NfObject.h"

namespace NeuroflowN
{
    class IDeviceArrayManagement : public NfObject
    {
    public:
        virtual IDeviceArray* CreateArray(bool copyOptimized, int size) = 0;

        virtual IDeviceArray2* CreateArray2(bool copyOptimized, int rowSize, int colSize) = 0;

        virtual void Copy(IDeviceArray* from, int fromIndex, IDeviceArray* to, int toIndex, int size) = 0;
    };
}