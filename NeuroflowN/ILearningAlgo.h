#pragma once
#include "Typedefs.h"
#include "NNMetadata.h"
#include "NfObject.h"

namespace NeuroflowN
{
    class ILearningAlgo : public NfObject
    {
    public:
        virtual LearningAlgoIterationType GetIterationTypes() = 0;

        virtual void Initalize() = 0;

        virtual void Run(int iterationCount, IDeviceArray* error) = 0;
    };
}