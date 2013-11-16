#pragma once

#include "Typedefs.h"
#include <assert.h>

namespace Neuroflow
{
    ref class NativeDeviceArrayManagement : Neuroflow::IDeviceArrayManagement
    {
        NeuroflowN::IDeviceArrayManagement* deviceArrayManagement;

    public:
        NativeDeviceArrayManagement(NeuroflowN::IDeviceArrayManagement* deviceArrayManagement) :
            deviceArrayManagement(deviceArrayManagement)
        {
            assert(deviceArrayManagement != nullptr);
        }

        virtual IDeviceArray^ CreateArray(bool copyOptimized, int size);

        virtual IDeviceArray2^ CreateArray2(bool copyOptimized, int rowSize, int colSize);

        virtual void Copy(IDeviceArray^ from, int fromIndex, IDeviceArray^ to, int toIndex, int size);

        virtual IDeviceArrayPool^ CreatePool()
        {
            throw gcnew System::NotImplementedException();
        }
    };
}
