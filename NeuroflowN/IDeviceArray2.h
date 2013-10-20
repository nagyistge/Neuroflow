#pragma once

#include "Typedefs.h"
#include "IDeviceArray.h"

namespace NeuroflowN
{
    class IDeviceArray2 : public IDeviceArray
    {
    public:
        virtual unsigned GetSize1() const = 0;
        virtual unsigned GetSize2() const = 0;
    };
}