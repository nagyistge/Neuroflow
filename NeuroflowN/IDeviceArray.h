#pragma once

#include "Typedefs.h"
#include "NfObject.h"

namespace NeuroflowN
{
    enum class DeviceArrayType : byte
    {
        DeviceArray,
        DeviceArray2,
        DataArray
    };

    class IDeviceArray : public NfObject
    {
    public:
        virtual DeviceArrayType GetType() const = 0;
        virtual unsigned GetSize() const = 0;
    };
}
