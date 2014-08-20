#pragma once

#include "Typedefs.h"
#include "NfObject.h"

namespace NeuroflowN
{
    class IVectorUtils : public NfObject
    {
    public:
        virtual void RandomizeUniform(IDeviceArray* values, float min, float max) = 0;

        virtual void CalculateMSE(const SupervisedBatchT& batch, DataArray* mseValues, unsigned valueIndex) = 0;

        virtual void Zero(IDeviceArray* deviceArray) = 0;
    };
}