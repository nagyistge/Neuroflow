#include "Stdafx.h"
#include "MUtil.h"
#include "Callbacks.h"
#include "NativeList.h"
#include "NativeObject.h"
#include "NativeDataArray.h"
#include "NativeDataArrayFactory.h"
#include "NativeDeviceArray.h"
#include "NativeDeviceArray2.h"
#include "NativePtr.h"

using namespace System::Runtime::InteropServices;
using namespace std;
using namespace Neuroflow;
using namespace Neuroflow::NeuralNetworks;
using namespace System;

std::string Neuroflow::GetVerison()
{
    auto v = System::Reflection::Assembly::GetCallingAssembly()->GetName()->Version->ToString();
    return msclr::interop::marshal_as<string>(v);
}

NeuroflowN::DataArray* Neuroflow::ToNative(Neuroflow::Data::DataArray^ dataArray, bool nullable)
{
    if (dataArray == null)
    {
        if (nullable) return null;
        throw gcnew ArgumentNullException("dataArray");
    }

    return ((Data::NativeDataArray^)dataArray)->PDataArray;
}

NeuroflowN::SupervisedBatchT Neuroflow::ToNative(Data::SupervisedBatch^ batch)
{
    NeuroflowN::SupervisedBatchT nbatch;

    for each (Data::SupervisedSample^ sample in batch)
    {
        NeuroflowN::SupervisedSampleT nsample;
		for each (Data::SupervisedSampleEntry^ entry in sample)
        {
            nsample.emplace_back(make_tuple(ToNative(entry->Input), ToNative(entry->DesiredOutput, true), ToNative(entry->ActualOutput, true)));
        }
        nbatch.push_back(move(nsample));
    }

    return move(nbatch);
}

NeuroflowN::IDeviceArray* Neuroflow::ToNative(IDeviceArray^ deviceArray)
{
    if (deviceArray == null) throw gcnew ArgumentNullException("deviceArray");

    switch (deviceArray->Type)
    {
    case DeviceArrayType::DeviceArray:
        return ((NativeDeviceArray^)deviceArray)->PDeviceArray;
    case DeviceArrayType::DeviceArray2:
        return ((NativeDeviceArray2^)deviceArray)->PDeviceArray2;
    case DeviceArrayType::DataArray:
        return ((Data::NativeDataArray^)deviceArray)->PDataArray;
    }

    throw gcnew ArgumentException("DeviceArray type " + deviceArray->GetType()->FullName + " is unknown.", "deviceArray");
}

NeuroflowN::IDeviceArray2* Neuroflow::ToNative(IDeviceArray2^ deviceArray)
{
    if (deviceArray == null) throw gcnew ArgumentNullException("deviceArray");

    return ((NativeDeviceArray2^)deviceArray)->PDeviceArray2;
}

std::function<NeuroflowN::IDeviceArray*()> Neuroflow::ToNative(DeviceArrayFactory^ deviceArrayF)
{
    if (deviceArrayF == null) throw gcnew ArgumentNullException("deviceArrayF");

    auto fp = Marshal::GetFunctionPointerForDelegate(deviceArrayF).ToPointer();
    return move(DeviceArrayFunc((DeviceArrayFuncFnc*)fp));
}

NeuroflowN::DeviceArrayFVecT* Neuroflow::ToNative(Marshaled<array<DeviceArrayFactory^>^>^ deviceArraysM)
{
    if (deviceArraysM == null || deviceArraysM->ManagedObject == null) return null;
    if (deviceArraysM->NativeVersion != null)
    {
        return ((NativePtr<NeuroflowN::DeviceArrayFVecT>^)deviceArraysM->NativeVersion)->Ptr;
    }

    auto deviceArrays = deviceArraysM->ManagedObject;
    auto result = new NeuroflowN::DeviceArrayFVecT();
    deviceArraysM->NativeVersion = gcnew NativePtr<NeuroflowN::DeviceArrayFVecT>(result);
    result->reserve(deviceArrays->Length);
    for each (DeviceArrayFactory^ deviceArrayF in deviceArrays)
    {
        result->push_back(ToNative(deviceArrayF));
    }
    return result;
}

NeuroflowN::DeviceArrayVecT* Neuroflow::ToNative(Marshaled<array<IDeviceArray^>^>^ deviceArraysM)
{
    if (deviceArraysM == null || deviceArraysM->ManagedObject == null) return null;
    if (deviceArraysM->NativeVersion != null)
    {
        return ((NativePtr<NeuroflowN::DeviceArrayVecT>^)deviceArraysM->NativeVersion)->Ptr;
    }

    auto deviceArrays = deviceArraysM->ManagedObject;
    auto result = new NeuroflowN::DeviceArrayVecT();
    deviceArraysM->NativeVersion = gcnew NativePtr<NeuroflowN::DeviceArrayVecT>(result);
    result->reserve(deviceArrays->Length);
    for each (IDeviceArray^ deviceArray in deviceArrays)
    {
        result->push_back(ToNative(deviceArray));
    }
    return result;
}

NeuroflowN::DeviceArray2VecT* Neuroflow::ToNative(Marshaled<array<IDeviceArray2^>^>^ deviceArraysM)
{
    if (deviceArraysM == null || deviceArraysM->ManagedObject == null) return null;
    if (deviceArraysM->NativeVersion != null)
    {
        return ((NativePtr<NeuroflowN::DeviceArray2VecT>^)deviceArraysM->NativeVersion)->Ptr;
    }

    auto deviceArrays = deviceArraysM->ManagedObject;
    auto result = new NeuroflowN::DeviceArray2VecT();
    deviceArraysM->NativeVersion = gcnew NativePtr<NeuroflowN::DeviceArray2VecT>(result);
    result->reserve(deviceArrays->Length);
    for each (IDeviceArray2^ deviceArray in deviceArrays)
    {
        result->push_back(ToNative(deviceArray));
    }
    return result;
}

NeuroflowN::WeightUpdateMode Neuroflow::ToNative(WeigthUpdateMode mode)
{
    return mode == WeigthUpdateMode::Offline ? NeuroflowN::WeightUpdateMode::Offline : NeuroflowN::WeightUpdateMode::Online;
}

NeuroflowN::ActivationFunction Neuroflow::ToNative(ActivationFunction function)
{
    return function == ActivationFunction::Linear ? NeuroflowN::ActivationFunction::Linear : NeuroflowN::ActivationFunction::Sigmoid;
}

NeuroflowN::LearningBehaviorSPtrT Neuroflow::ToNative(NeuralNetworks::LearningBehavior^ behavior)
{
    //// UniformRandomizeWeights
    //auto uniRndW = dynamic_cast<UniformRandomizeWeights^>(behavior);
    //if (uniRndW != null)
    //{
    //    return static_pointer_cast<NeuroflowN::LearningBehavior>(make_shared<NeuroflowN::UniformRandomizeWeights>(uniRndW->Strength));
    //}

    // GradientDescentLearningRule
    auto gdLr = dynamic_cast<GradientDescentLearningRule^>(behavior);
    if (gdLr != nullptr)
    {
        return static_pointer_cast<NeuroflowN::LearningBehavior>(make_shared<NeuroflowN::GradientDescentLearningRule>(gdLr->GroupID, gdLr->LearningRate, gdLr->Momentum, gdLr->Smoothing, ToNative(gdLr->WeightUpdateMode)));
    }

    throw gcnew NotSupportedException("Learning of '" + behavior->GetType()->Name + "' not supported.");
}

NeuroflowN::TrainingNodeVecT Neuroflow::ToNative(System::Collections::ObjectModel::ReadOnlyCollection<NeuralNetworks::TrainingNode^>^ nodes)
{
    NeuroflowN::TrainingNodeVecT result;
    for each (NeuralNetworks::TrainingNode^ node in nodes)
    {
        result.emplace_back(ToNative(node->Weights), node->Gradients != null ? make_shared<NeuroflowN::DeviceArrayVecT>(ToNative(node->Gradients)) : null, node->GradientSums != null ? make_shared<NeuroflowN::DeviceArrayVecT>(ToNative(node->GradientSums)) : null);
    }
    return move(result);
}

NeuroflowN::DeviceArrayVecT Neuroflow::ToNative(System::Collections::ObjectModel::ReadOnlyCollection<IDeviceArray^>^ arrays)
{
    NeuroflowN::DeviceArrayVecT result;
    for each (IDeviceArray^ da in arrays)
    {
        result.emplace_back(ToNative(da));
    }
    return move(result);
}

NeuroflowN::NfObject* Neuroflow::ToNative(System::IDisposable^ obj)
{
    if (obj == null) throw gcnew ArgumentNullException("obj");

    return ((NativeObject^)obj)->PObj;
}

NeuralNetworks::LearningAlgoIterationType Neuroflow::ToManaged(NeuroflowN::LearningAlgoIterationType type)
{
    return (NeuralNetworks::LearningAlgoIterationType)((int) type);
}