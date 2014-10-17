#pragma once

#include <assert.h>
#include "Typedefs.h"

namespace Neuroflow
{
    ref class NativeVectorUtils : public Neuroflow::VectorUtils
    {
        NeuroflowN::IVectorUtils* vectorUtils;

    public:
        NativeVectorUtils(NeuroflowN::IVectorUtils* vectorUtils) :
            vectorUtils(vectorUtils)
        {
            assert(vectorUtils != null);
        }

        virtual void RandomizeUniform(Neuroflow::IDeviceArray^ values, float min, float max) override;

        virtual void Zero(IDeviceArray^ deviceArray) override;

    protected:

        virtual void DoCalculateMSE(Data::SupervisedBatch^ batch, Data::DataArray^ mseValues, int valueIndex) override;
    };
}