#include "stdafx.h"
#include "NativeLearningAlgoFactory.h"
#include "ILearningAlgoFactory.h"
#include "NativeLearningAlgo.h"
#include "MUtil.h"
#include "NativeException.h"

using namespace std;
using namespace Neuroflow;
using namespace Neuroflow::NeuralNetworks;

ILearningAlgo^ NativeLearningAlgoFactory::CreateLearningAlgo(LearningBehavior^ learningBehavior, System::Collections::ObjectModel::ReadOnlyCollection<TrainingNode^>^ nodes)
{
    try
    {
        return gcnew NativeLearningAlgo(learningAlgoFactory->CreateLearningAlgo(ToNative(learningBehavior), ToNative(nodes)));
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}