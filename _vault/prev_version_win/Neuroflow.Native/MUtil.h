#pragma once

#include <vector>
#include <string>
#include "NativeTypedefs.h"
#include "NNMetadata.h"

namespace Neuroflow
{
    std::string GetVerison();

    NeuroflowN::DataArray* ToNative(Data::DataArray^ dataArray, bool nullable = false);

    NeuroflowN::SupervisedBatchT ToNative(Data::SupervisedBatch^ batch);

    NeuroflowN::DeviceArrayFactoryT ToNative(DeviceArrayFactory^ deviceArrayF);

    NeuroflowN::IDeviceArray* ToNative(IDeviceArray^ deviceArray);

    NeuroflowN::IDeviceArray2* ToNative(IDeviceArray2^ deviceArray);

    NeuroflowN::DeviceArrayFVecT* ToNative(Marshaled<array<DeviceArrayFactory^>^>^ deviceArrays);

    NeuroflowN::DeviceArrayVecT* ToNative(Marshaled<array<IDeviceArray^>^>^ deviceArrays);

    NeuroflowN::DeviceArray2VecT* ToNative(Marshaled<array<IDeviceArray2^>^>^ deviceArrays);

    NeuroflowN::WeightUpdateMode ToNative(NeuralNetworks::WeigthUpdateMode mode);

    NeuroflowN::ActivationFunction ToNative(NeuralNetworks::ActivationFunction function);

    NeuroflowN::LearningBehaviorSPtrT ToNative(NeuralNetworks::LearningBehavior^ behavior);

    NeuroflowN::TrainingNodeVecT ToNative(System::Collections::ObjectModel::ReadOnlyCollection<NeuralNetworks::TrainingNode^>^ nodes);

    NeuroflowN::DeviceArrayVecT ToNative(System::Collections::ObjectModel::ReadOnlyCollection<IDeviceArray^>^ arrays);

    NeuroflowN::NfObject* ToNative(System::IDisposable^ obj);

    NeuralNetworks::LearningAlgoIterationType ToManaged(NeuroflowN::LearningAlgoIterationType type);

    NeuroflowN::RTLRLayerInfoVecVecT* ToNative(Marshaled<array<array<NeuralNetworks::RTLRLayerInfo^>^>^>^ inputLayerInfos);

    NeuroflowN::RTLRComputationData* ToNative(Marshaled<NeuralNetworks::RTLRComputationData^>^ data);

    NeuroflowN::RTLRComputationData2* ToNative(Marshaled<NeuralNetworks::RTLRComputationData2^>^ data);
}