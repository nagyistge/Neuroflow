#pragma once

#include "Typedefs.h"
#include <assert.h>

namespace Neuroflow
{
    namespace NeuralNetworks
    {
        ref class NativeLearningAlgo : public ILearningAlgo
        {
            NeuroflowN::ILearningAlgo* learningAlgo;

        public:
            NativeLearningAlgo(NeuroflowN::ILearningAlgo* learningAlgo) :
                learningAlgo(learningAlgo)
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