#include "Stdafx.h"
#include "NativeMultilayerPerceptronAdapter.h"
#include "IMultilayerPerceptronAdapter.h"
#include "NativeDeviceArrayManagement.h"
#include "NativeVectorUtils.h"
#include "NativeComputeActivation.h"
#include "NativeLearningAlgoFactory.h"
#include "NativeException.h"

using namespace std;
using namespace Neuroflow;
using namespace Neuroflow::NeuralNetworks;

NativeMultilayerPerceptronAdapter::NativeMultilayerPerceptronAdapter(NeuroflowN::IMultilayerPerceptronAdapter* adapter) :
    adapter(adapter)
{
    assert(adapter != nullptr);

    try
    {
        deviceArrayManagement = gcnew NativeDeviceArrayManagement(adapter->GetDeviceArrayManagementPtr());
        vectorUtils = gcnew NativeVectorUtils(adapter->GetVectorUtilsPtr());
        computeActivation = gcnew NativeComputeActivation(adapter->GetComputeActivationPtr());
        learningAlgoFactory = gcnew NativeLearningAlgoFactory(adapter->GetLearningAlgoFactoryPtr());
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}