#pragma once

#include <vector>
#include <vcclr.h>
#include "Factory.h"
#include <boost/optional.hpp>

using namespace System;
using namespace System::Collections::Generic;
using namespace Neuroflow::Networks::Neural;
using namespace Neuroflow::Core::Vectors;
using namespace System::Diagnostics;
using namespace std;

namespace Neuroflow
{
namespace Native 
{
    ref class NativeNeuralNetwork;

    public ref class NativeNNInitParameters : public NNInitParameters
    {
    public:
        virtual property Type^ NeuralNetworkType
        {
            Type^ get() override
            { 
                return NativeNeuralNetwork::typeid; 
            } 
        }
    }; 

    public ref class NativeNeuralComputationContext : public NeuralComputationContext
    {
        Disposable* data;
    public:
        NativeNeuralComputationContext(Disposable* data) : 
            data(data), 
            NeuralComputationContext() 
        { 
        }
    protected:
        virtual void FreeUnmanagedResources() override
        {
            delete data;
            data = nullptr;
        }
    public protected:
        Disposable* GetContext() { return data; }
    };

    class NativeVectorBufferInvoker
    {
        gcroot<Func<array<float>^>^> f;
    public:
        NativeVectorBufferInvoker(gcroot<Func<array<float>^>^> f) : f(f) { }
        vector<float> operator()()
        {
            auto a = f->Invoke();
            auto r = vector<float>();
            for each (float v in a)
            {
                r.push_back(v);
            }
            return move(r);
        }
    };

    public ref class NativeVectorBuffer : public Neuroflow::Core::Vectors::VectorBuffer<float>
    {
        IVectorBuffer* data;
    public:
        NativeVectorBuffer(IVectorBuffer* data) : 
            data(data), 
            Neuroflow::Core::Vectors::VectorBuffer<float>() 
        { 
        }
    protected:
        virtual Neuroflow::Core::Vectors::BufferedVector<float>^ DoGetOrCreate(int rowIndex, int colIndex, Func<array<float>^>^ valuesFactory) override
        {
            int size = data->Create(rowIndex, colIndex, NativeVectorBufferInvoker(gcroot<Func<array<float>^>^>(valuesFactory)));
            return CreateVector(rowIndex, colIndex, size, nullptr); 
        }

        virtual void FreeUnmanagedResources() override
        {
            auto disp = dynamic_cast<Disposable*>(data);
            if (disp)
            {
                delete disp;
                data = nullptr;
            }
        }
    public protected:
        IVectorBuffer* GetVectorBuffer() { return data; }
    };

    public ref class NativeNeuralNetwork : public NeuralNetwork
    {
    public:
		NativeNeuralNetwork(ICollection<ConnectableLayer^>^ layers)
			: NeuralNetwork(layers, gcnew NativeNNInitParameters(), NNAlgorithm::None)
        {
        }

        NativeNeuralNetwork(ICollection<ConnectableLayer^>^ layers, NativeNNInitParameters^ parameters)
            : NeuralNetwork(layers, parameters, NNAlgorithm::None)
        {
        }

		NativeNeuralNetwork(ICollection<ConnectableLayer^>^ layers, NNAlgorithm supportedAlgorithms)
			: NeuralNetwork(layers, gcnew NativeNNInitParameters(), supportedAlgorithms)
        {
        }

        NativeNeuralNetwork(ICollection<ConnectableLayer^>^ layers, NativeNNInitParameters^ parameters, NNAlgorithm supportedAlgorithms)
            : NeuralNetwork(layers, parameters, supportedAlgorithms)
        {
        }

    private:
        int allocatedBuffSize;
        INativeNeuralNetworkImpl* impl;

    protected:
        virtual void Build(BufferAllocator^ allocator, ConnectedLayerGroups^ connectedLayerGroups, NNInitParameters^ initPars) override
        {
            impl = Factory::CreateCPPAmpNN();

            impl->InitializeInputAndOutput(
                IntRange(connectedLayerGroups->InputBuffer.MinValue, connectedLayerGroups->InputBuffer.MaxValue), 
                IntRange(connectedLayerGroups->OutputBuffer.MinValue, connectedLayerGroups->OutputBuffer.MaxValue));

            BuildForwardComputation(allocator, connectedLayerGroups, (NativeNNInitParameters^)initPars);
            //if (IsBackwardEnabled) BuildBackwardComputation(connectedLayerGroups);
        }

        virtual void Built(BufferAllocator^ allocator, ConnectedLayerGroups^ connectedLayerGroups, NNInitParameters^ initPars) override
        {
            allocatedBuffSize = allocator->Size;
        }

        virtual void InitializeLearningAlgorithms(BufferAllocator^ allocator, Neuroflow::Networks::Neural::Learning::LearningLayerGroups^ learningLayerGroups, NNInitParameters^ initPars) override
        {
        }

        virtual NeuralComputationContext^ DoCreateContext() override
        {
            Disposable* buff = nullptr;
            try
            {
                buff = impl->CreateContext(allocatedBuffSize);
                return gcnew NativeNeuralComputationContext(buff);
            }
            catch (...)
            {
                delete buff;
                throw;
            }
        }

        virtual Neuroflow::Core::Vectors::VectorBuffer<float>^ DoCreateVectorBuffer() override
        {
            IVectorBuffer* buff = nullptr;
            try
            {
                buff = impl->CreateVectorBuffer();
                return gcnew NativeVectorBuffer(buff);
            }
            catch (...)
            {
                delete buff;
                throw;
            }
        }

        virtual void DoIteration(NeuralComputationContext^ context, bool collectTrainingData, Nullable<int> innerIterationIndex) override
        {
            auto ctx = GetContext(context);
        }

        virtual void DoBackpropagate(NeuralComputationContext^ context, BackprogrationMode mode, Nullable<int> innerIterationIndex) override
        {
            auto ctx = GetContext(context);
        }

		virtual void DoPropagatePValues(NeuralComputationContext^ context, Nullable<Neuroflow::Core::IntRange> errorBuffer) override
        {
            auto ctx = GetContext(context);
        }

        virtual void DoWriteInput(NeuralComputationContext^ context, Neuroflow::Core::Vectors::BufferedVector<float>^ values) override
        {
            auto ctx = GetContext(context);
            auto natOwner = GetNativeOwner(values);
            impl->WriteInput(ctx, natOwner, VectorHandle(values->RowIndex, values->ColIndex, values->Length));
        }

        virtual void DoReadOutput(NeuralComputationContext^ context, array<float>^ values) override
        {
            auto ctx = GetContext(context);
            pin_ptr<float> p = &values[0];
            impl->ReadOutput(ctx, p);
        }

		virtual void DoComputeError(NeuralComputationContext^ context, Neuroflow::Core::Vectors::BufferedVector<float>^ desiredOutputVector, Neuroflow::Core::IntRange errorBuffer, Neuroflow::Core::IntRange accumulationBuffer) override
		{
			auto ctx = GetContext(context);
            auto natOwner = GetNativeOwner(desiredOutputVector);
		}

        virtual void DoSetError(NeuralComputationContext^ context, Neuroflow::Core::IntRange errorBuffer) override
        {
            auto ctx = GetContext(context);
        }

		virtual void DoReadError(NeuralComputationContext^  context, array<float>^ values, Neuroflow::Core::IntRange errorBuffer) override
		{
            auto ctx = GetContext(context);
        }

		virtual void DoZeroBuffer(NeuralComputationContext^  context, Neuroflow::Core::IntRange accumulationBuffer) override
		{
            auto ctx = GetContext(context);
        }

		virtual void DoCalculateAverageError(NeuralComputationContext^  context, Neuroflow::Core::IntRange accumulationBuffer) override
		{
            auto ctx = GetContext(context);
        }

		virtual void DoCopyBuffer(NeuralComputationContext^  context, Neuroflow::Core::IntRange source, Neuroflow::Core::IntRange target) override
		{
            auto ctx = GetContext(context);
        }

        virtual void ResetAlgorithms(NeuralComputationContext^ context) override
        {
            auto ctx = GetContext(context);
        }

        virtual void ResetAll(NeuralComputationContext^ context) override
        {
            auto ctx = GetContext(context);
            impl->ResetAll(ctx);
        }

        virtual void ResetForwardValues(NeuralComputationContext^ context,Neuroflow::Networks::Neural::NeuralNetworkResetTarget target) override
        {
            auto ctx = GetContext(context);
            impl->ResetForwardValues(ctx, ToResetTarget(target));
        }

        virtual void ResetBackwardValues(NeuralComputationContext^ context, Neuroflow::Networks::Neural::NeuralNetworkResetTarget target) override
        {
            auto ctx = GetContext(context);
        }

        virtual void FreeUnmanagedResources() override
        {
            delete impl;
            impl = nullptr;
        }

    public protected:
        virtual void DoCallBeforeIterationLearningAlgorithms(NeuralComputationContext^ context, bool isNewBatch) override
        {
            auto ctx = GetContext(context);
        }

		virtual void DoCallErrorBasedBatchLearningAlgorithms(NeuralComputationContext^ context, int batchSize, Neuroflow::Core::IntRange errorBuffer) override
        {
            auto ctx = GetContext(context);
        }

        virtual void DoCallErrorBasedStochasticLearningAlgorithms(NeuralComputationContext^ context, Neuroflow::Core::IntRange errorBuffer) override
        {
            auto ctx = GetContext(context);
        }

    private:
#pragma region Build Forward

        void BuildForwardComputation(BufferAllocator^ allocator, ConnectedLayerGroups^ connectedLayerGroups, NativeNNInitParameters^ initPars)
        {
            int groupsCount = connectedLayerGroups->Groups->Count;
            auto forwardComputeGroups = LayerForwardComputeGroups();
            for (int groupIndex = 0; groupIndex < groupsCount; groupIndex++)
            {
                auto group = connectedLayerGroups->Groups->default[groupIndex];
                int groupCount = group->Count;
                auto computes = LayerForwardComputes();
                for (int layerIndex = 0; layerIndex < groupCount; layerIndex++)
                {
                    auto comp = CreateLayerForwardCompute(allocator, group->default[layerIndex], initPars);
                    computes.push_back(move(comp));
                }
                forwardComputeGroups.push_back(move(computes));                
            }
            impl->StoreLayerForwardComputeGroups(move(forwardComputeGroups));
        }

        UPLayerForwardCompute CreateLayerForwardCompute(BufferAllocator^ allocator, ConnectedLayer^ clayer, NativeNNInitParameters^ initPars)
        {
            UPLayerForwardCompute result;
            if (dynamic_cast<ActivationLayer^>(clayer->Layer) != nullptr)
            {
                result = move(UPLayerForwardCompute(new LayerForwardCompute())); // TODO: ActivationLayerForwardCompute
            }

            if (!result) throw gcnew InvalidOperationException("Cannot build Managed Neural Network, because '" + clayer->Layer->GetType()->FullName + "' layer type is unknown.");

            InitializeLayerForwardCompute(result, clayer);

            return move(result);
        }

        void InitializeLayerForwardCompute(UPLayerForwardCompute& comp, ConnectedLayer^ connectedLayer)
        {
            comp->ConnectedLayerIndex = connectedLayer->Index;
            InitInputValueAccessItems(comp, connectedLayer);
            comp->OutputBuffer = ToIntRange(connectedLayer->OutputBuffer);
            comp->BiasValueIndex = ToOptionalInt(connectedLayer->BiasValueIndex);
            comp->InnerItarationOutputValueStack = ToIntRangeVector(connectedLayer->InnerItarationOutputValueStack);
            comp->IsOutput = connectedLayer->IsOutput;
            if ((int)(connectedLayer->StructuralElementFlags & NNStructuralElement::RTLRInformation) != 0)
            {
                comp->Method = ToMethod(Neuroflow::Networks::Neural::ForwardComputationMethod::RTLR);
                comp->UpperNonInputLayerInfos = ToUpperLayerInfoVector(connectedLayer->UpperNonInputLayerInfos);
                comp->PBiasBuffers = ToIntRangeVector(connectedLayer->PBiasBuffers);
                comp->PWeightBuffers = ToIntRangeVectorVectorVector(connectedLayer->PWeightBuffers);
                comp->PrevPBiasBuffers = ToIntRangeVector(connectedLayer->PrevPBiasBuffers);
                comp->PrevPWeightBuffers = ToIntRangeVectorVectorVector(connectedLayer->PrevPWeightBuffers);
				comp->NetDerivBuffer = 
					connectedLayer->NetDerivBuffer.HasValue ? 
					boost::optional<IntRange>(IntRange(connectedLayer->NetDerivBuffer.Value.MinValue, connectedLayer->NetDerivBuffer.Value.MaxValue)) : 
					boost::optional<IntRange>();
                comp->GradientBuffers = ToIntRangeVector(connectedLayer->GradientBuffers);
                comp->GradientSumBuffers = ToIntRangeVector(connectedLayer->GradientSumBuffers);
                comp->BiasGradientValueIndex = ToOptionalInt(connectedLayer->BiasGradientValueIndex);
                comp->BiasGradientSumValueIndex = ToOptionalInt(connectedLayer->BiasGradientSumValueIndex);
            }
            else if (connectedLayer->InnerItarationOutputValueStack != nullptr)
            {
                Debug::Assert(connectedLayer->InnerItarationInputValueStacks != nullptr);

                comp->Method = ToMethod(Neuroflow::Networks::Neural::ForwardComputationMethod::BPTT);
            }
            else
            {
                comp->Method = ToMethod(Neuroflow::Networks::Neural::ForwardComputationMethod::FeedForward);
            }
        } 

        void InitInputValueAccessItems(UPLayerForwardCompute& comp, ConnectedLayer^ connectedLayer)
        {
            int count = connectedLayer->WeightedInputBuffers->Length;
            auto items = vector<InputValueAccess>(count);

            for (int inputBufferIndex = 0; inputBufferIndex < count; inputBufferIndex++)
            {
                auto weightedInputBuffer = connectedLayer->WeightedInputBuffers[inputBufferIndex];
                int wibValueBufferSize = weightedInputBuffer.ValueBuffer.Size;
                items[inputBufferIndex] =
                    InputValueAccess(
                        weightedInputBuffer.ValueBuffer.Size,
                        weightedInputBuffer.ValueBuffer.MinValue,
                        weightedInputBuffer.WeightBuffer.MinValue,
                        connectedLayer->InnerItarationInputValueStacks != nullptr ? ToIntRangeVector(connectedLayer->InnerItarationInputValueStacks[inputBufferIndex]) : vector<IntRange>());
            }

            comp->InputValueAccessItems = items;
        }

#pragma endregion

#pragma region Conv

    private:
        const ::NeuralNetworkResetTarget ToResetTarget(Neuroflow::Networks::Neural::NeuralNetworkResetTarget target)
        {
            switch (target)
            {
            case Neuroflow::Networks::Neural::NeuralNetworkResetTarget::Outputs:
                return ::NeuralNetworkResetTarget::Outputs;
            case Neuroflow::Networks::Neural::NeuralNetworkResetTarget::Errors:
                return ::NeuralNetworkResetTarget::Errors;
            case Neuroflow::Networks::Neural::NeuralNetworkResetTarget::Gradients:
                return ::NeuralNetworkResetTarget::Gradients;
            case Neuroflow::Networks::Neural::NeuralNetworkResetTarget::GradientSums:
                return ::NeuralNetworkResetTarget::GradientSums;
            case Neuroflow::Networks::Neural::NeuralNetworkResetTarget::Ps:
                return ::NeuralNetworkResetTarget::Ps;
            case Neuroflow::Networks::Neural::NeuralNetworkResetTarget::Algorithms:
                return ::NeuralNetworkResetTarget::Algorithms;
            case Neuroflow::Networks::Neural::NeuralNetworkResetTarget::All:
                return ::NeuralNetworkResetTarget::All;
            default:
                throw gcnew InvalidOperationException("Unknown NeuralNetworkResetTarget: " + target.ToString() + ".");
            }
        }

        const vector<::UpperLayerInfo> ToUpperLayerInfoVector(array<Neuroflow::Networks::Neural::UpperLayerInfo>^ infos)
        {
            if (infos == nullptr) return vector<::UpperLayerInfo>();

            auto v = vector<::UpperLayerInfo>(infos->Length);
            for (size_t i = 0; i < v.size(); i++)
            {
                v[i] = ToUpperLayerInfo(infos[i]);
            }
            return v;
        }

        const ::UpperLayerInfo ToUpperLayerInfo(Neuroflow::Networks::Neural::UpperLayerInfo^ info)
        {
            return ::UpperLayerInfo(info->WeightedErrorBufferIndex, info->LayerIndex);
        }

        const vector<vector<vector<IntRange>>> ToIntRangeVectorVectorVector(array<array<array<Neuroflow::Core::IntRange>^>^>^ ranges)
        {
            if (ranges == nullptr) return vector<vector<vector<IntRange>>>();

            auto v = vector<vector<vector<IntRange>>>(ranges->Length);
            for (size_t i = 0; i < v.size(); i++)
            {
                v[i] = ToIntRangeVectorVector(ranges[i]);
            }
            return v;
        }

        const vector<vector<IntRange>> ToIntRangeVectorVector(array<array<Neuroflow::Core::IntRange>^>^ ranges)
        {
            if (ranges == nullptr) return vector<vector<IntRange>>();

            auto v = vector<vector<IntRange>>(ranges->Length);
            for (size_t i = 0; i < v.size(); i++)
            {
                v[i] = ToIntRangeVector(ranges[i]);
            }
            return v;
        }

        const vector<IntRange> ToIntRangeVector(array<Neuroflow::Core::IntRange>^ ranges)
        {
            if (ranges == nullptr) return vector<IntRange>();

            auto v = vector<IntRange>(ranges->Length);
            for (size_t i = 0; i < v.size(); i++)
            {
                v[i] = ToIntRange(ranges[i]);
            }
            return v;
        }

        const IntRange ToIntRange(Neuroflow::Core::IntRange^ range)
        {
            return IntRange(range->MinValue, range->MaxValue);
        }

		const boost::optional<int> ToOptionalInt(Nullable<int>^ i)
        {
            return i->HasValue ? boost::optional<int>(i->Value) : boost::optional<int>();
        }

        const ::ForwardComputationMethod ToMethod(Neuroflow::Networks::Neural::ForwardComputationMethod m)
        {
            switch (m)
            {
            case Neuroflow::Networks::Neural::ForwardComputationMethod::FeedForward:
                return ::ForwardComputationMethod::FeedForward;
            case Neuroflow::Networks::Neural::ForwardComputationMethod::BPTT:
                return ::ForwardComputationMethod::BPTT;
            case Neuroflow::Networks::Neural::ForwardComputationMethod::RTLR:
                return ::ForwardComputationMethod::RTLR;
            default:
                throw gcnew InvalidOperationException("Unknown ForwardComputationMethod: " + m.ToString() + ".");
            }
        }  

#pragma endregion

#pragma region Helpers
        private:
            Disposable* GetContext(NeuralComputationContext^ context)
            {
                return ((NativeNeuralComputationContext^)context)->GetContext();
            }  

            IVectorBuffer* GetNativeOwner(Neuroflow::Core::Vectors::BufferedVector<float>^ values)
            {
                auto owner = dynamic_cast<NativeVectorBuffer^>(values->Owner);
                if (owner == nullptr) throw gcnew ArgumentException("Vector's owner is not a NativeVectorBuffer object.", "values");
                auto natOwner = owner->GetVectorBuffer();
                return natOwner;
            }
#pragma endregion

    }; 
}
}