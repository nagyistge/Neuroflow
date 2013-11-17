#pragma once

#include "Typedefs.h"
#include <assert.h>
#include "NativePtr.h"

namespace Neuroflow
{
    namespace NeuralNetworks
    {
        ref class NativeLearningAlgo : public NativePtr<NeuroflowN::ILearningAlgo>, public ILearningAlgo
        {
        public:
            NativeLearningAlgo(NeuroflowN::ILearningAlgo* learningAlgo) :
                NativePtr(learningAlgo)
            {
                assert(learningAlgo != null);
            }

            virtual property LearningAlgoIterationType IterationTypes
            {
                LearningAlgoIterationType get()
                {
                    return GetIterationTypes();
                }
            }

            virtual void Initialize();

            virtual void Run(int iterationCount, IDeviceArray^ error);

        private:
            LearningAlgoIterationType GetIterationTypes();
        };
    }
}