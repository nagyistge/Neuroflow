#pragma once

#include "NfObject.h"
#include "Typedefs.h"

namespace NeuroflowN
{
    class IMultilayerPerceptronAdapter : public NfObject
    {
    public:
        virtual IDeviceArrayManagement* GetDeviceArrayManagementPtr() const = 0;

        virtual IVectorUtils* GetVectorUtilsPtr() const = 0;

        virtual IComputeActivation* GetComputeActivationPtr() const = 0;

        virtual ILearningAlgoFactory* GetLearningAlgoFactoryPtr() const = 0;
    };
}