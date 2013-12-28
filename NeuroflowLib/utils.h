#pragma once

#include "nf.h"

namespace nf
{
    _interface utils : virtual nf_object
    {
        //virtual void RandomizeUniform(device_array_ptr& values, float min, float max) = 0;
        //virtual void CalculateMSE(const SupervisedBatchT& batch, DataArray* mseValues, unsigned valueIndex) = 0;
        virtual void zero(device_array_ptr& deviceArray) const = 0;
    };
}