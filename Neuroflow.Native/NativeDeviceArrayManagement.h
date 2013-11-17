#pragma once

#include "Typedefs.h"
#include <assert.h>
#include "NativePtr.h"

namespace Neuroflow
{
    ref class NativeDeviceArrayManagement : public NativePtr<NeuroflowN::IDeviceArrayManagement>, public IDeviceArrayManagement
    {
    public:
        NativeDeviceArrayManagement(NeuroflowN::IDeviceArrayManagement* deviceArrayManagement) :
            NativePtr(deviceArrayManagement, false)
        {
            assert(deviceArrayManagement != null);
        }

        virtual IDeviceArray^ CreateArray(bool copyOptimized, int size);

        virtual IDeviceArray2^ CreateArray2(bool copyOptimized, int rowSize, int colSize);

        virtual void Copy(IDeviceArray^ from, int fromIndex, IDeviceArray^ to, int toIndex, int size);

        virtual IDeviceArrayPool^ CreatePool();
    };
}
