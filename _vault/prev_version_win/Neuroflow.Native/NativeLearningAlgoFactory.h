#pragma once

#include "Typedefs.h"
#include <assert.h>
#include "NativePtr.h"

namespace Neuroflow
{
    namespace NeuralNetworks
    {
        ref class NativeLearningAlgoFactory : public NativePtr<NeuroflowN::ILearningAlgoFactory>, public ILearningAlgoFactory
        {
        public:
            NativeLearningAlgoFactory(NeuroflowN::ILearningAlgoFactory* learningAlgoFactory) :
                NativePtr(learningAlgoFactory, false)
            {
                assert(learningAlgoFactory != null);
            }

            virtual ILearningAlgo^ CreateLearningAlgo(LearningBehavior^ learningBehavior, System::Collections::ObjectModel::ReadOnlyCollection<TrainingNode^>^ nodes);
        };
    }
}
