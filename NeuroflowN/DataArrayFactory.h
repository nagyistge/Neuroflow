#pragma once

#include "Typedefs.h"
#include "DataArray.h"
#include "NfObject.h"
#include <memory>

namespace NeuroflowN
{
    class DataArrayFactory : public NfObject
    {
    public:
        virtual DataArray* Create(unsigned size, float fill) = 0;

        virtual DataArray* Create(float* values, unsigned beginPos, unsigned length) = 0;

        virtual DataArray* CreateConst(float* values, unsigned beginPos, unsigned length) = 0;
    };
}