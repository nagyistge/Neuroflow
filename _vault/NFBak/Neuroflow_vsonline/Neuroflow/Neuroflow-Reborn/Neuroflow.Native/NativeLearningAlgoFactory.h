#pragma once

#include "Typedefs.h"
#include <assert.h>

namespace Neuroflow
{
    namespace NeuralNetworks
    {
        ref class NativeLearningAlgoFactory : public ILearningAlgoFactory
        {
            NeuroflowN::ILearningAlgoFactory* learningAlgoFactory;

        public:
            NativeLearningAlgoFactory(NeuroflowN::ILearningAlgoFactory* learningAlgoFactory) :
                learningAlgoFactory(learningAlgoFactory)
            {
                assert(learningAlgoFactory != null);
            }

            virtual ILearningAlgo^ CreateLearningAlgo(LearningBehavior^ learningBehavior, System::Collections::ObjectModel::ReadOnlyCollection<TrainingNode^>^ nodes);
        };
    }
}
