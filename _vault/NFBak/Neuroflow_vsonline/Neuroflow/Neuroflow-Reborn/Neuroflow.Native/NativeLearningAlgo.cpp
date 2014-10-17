#include "stdafx.h"
#include "NativeLearningAlgo.h"
#include "ILearningAlgo.h"
#include "MUtil.h"
#include "NativeException.h"

using namespace std;
using namespace Neuroflow;
using namespace Neuroflow::NeuralNetworks;

void NativeLearningAlgo::Initialize()
{
    try
    {
        learningAlgo->Initalize();
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

void NativeLearningAlgo::Run(int iterationCount, IDeviceArray^ error)
{
    try
    {
        learningAlgo->Run(iterationCount, error != null ? ToNative(error) : null);
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

LearningAlgoIterationType NativeLearningAlgo::GetIterationTypes()
{
    try
    {
        return ToManaged(learningAlgo->GetIterationTypes());
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}