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
        Ptr->Initalize();
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
        Ptr->Run(iterationCount, error != null ? ToNative(error) : null);
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
        return ToManaged(Ptr->GetIterationTypes());
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}