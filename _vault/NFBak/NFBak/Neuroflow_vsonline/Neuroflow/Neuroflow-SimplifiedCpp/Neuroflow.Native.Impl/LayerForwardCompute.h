#pragma once

#include "Typedefs.h"
#include "Enums.h"
#include "IntRange.h"
#include "InputValueAccess.h"
#include "UpperLayerInfo.h"
#include <boost/optional.hpp>

class LayerForwardCompute
{
#pragma region Ctor / Dtor

public:
    LayerForwardCompute(void);

#pragma endregion

#pragma region Properties

private:
    int connectedLayerIndex;
public:
    const int GetConnectedLayerIndex() const { return connectedLayerIndex; }
    void SetConnectedLayerIndex(const int value) { connectedLayerIndex = value; }
    __declspec(property(get = GetConnectedLayerIndex, put = SetConnectedLayerIndex)) int ConnectedLayerIndex;

private:
    bool isOutput;
public:
    const bool GetIsOutput() const { return isOutput; }
    void SetIsOutput(const bool value) { isOutput = value; }
    __declspec(property(get = GetIsOutput, put = SetIsOutput)) bool IsOutput;

private:
    std::vector<InputValueAccess> inputValueAccessItems;
public:
    const std::vector<InputValueAccess>& GetInputValueAccessItems() { return inputValueAccessItems; }
    void SetInputValueAccessItems(const std::vector<InputValueAccess>& value) { inputValueAccessItems = value; }
    __declspec(property(get = GetInputValueAccessItems, put = SetInputValueAccessItems)) std::vector<InputValueAccess>& InputValueAccessItems;

private:
    IntRange outputBuffer;
public:
    IntRange& GetOutputBuffer() { return outputBuffer; }
    void SetOutputBuffer(const IntRange& value) { outputBuffer = value; }
    __declspec(property(get = GetOutputBuffer, put = SetOutputBuffer)) IntRange& OutputBuffer;

private:
    boost::optional<int> biasValueIndex;
public:
    const boost::optional<int>& GetBiasValueIndex() const { return biasValueIndex; }
    void SetBiasValueIndex(const boost::optional<int>& value) { biasValueIndex = value; }
    __declspec(property(get = GetBiasValueIndex, put = SetBiasValueIndex)) boost::optional<int>& BiasValueIndex;

private:
    std::vector<IntRange> innerItarationOutputValueStack;
public:
    const std::vector<IntRange>& GetInnerItarationOutputValueStack() const { return innerItarationOutputValueStack; }
    void SetInnerItarationOutputValueStack(const std::vector<IntRange>& value) { innerItarationOutputValueStack = value; }
    __declspec(property(get = GetInnerItarationOutputValueStack, put = SetInnerItarationOutputValueStack)) std::vector<IntRange>& InnerItarationOutputValueStack;

private:
    ::ForwardComputationMethod method;
public:
    const ::ForwardComputationMethod GetMethod() const { return method; }
    void SetMethod(const ::ForwardComputationMethod value) { method = value; }
    __declspec(property(get = GetMethod, put = SetMethod)) ::ForwardComputationMethod Method;

private:
    std::vector<::UpperLayerInfo> upperNonInputLayerInfos;
public:
    const std::vector<::UpperLayerInfo>& GetUpperNonInputLayerInfos() const { return upperNonInputLayerInfos; }
    void SetUpperNonInputLayerInfos(const std::vector<::UpperLayerInfo>& value) { upperNonInputLayerInfos = value; }
    __declspec(property(get = GetUpperNonInputLayerInfos, put = SetUpperNonInputLayerInfos)) std::vector<::UpperLayerInfo>& UpperNonInputLayerInfos;

private:
    std::vector<IntRange> pBiasBuffers;
public:
    const std::vector<IntRange>& GetPBiasBuffers() const { return pBiasBuffers; }
    void SetPBiasBuffers(const std::vector<IntRange>& value) { pBiasBuffers = value; }
    __declspec(property(get = GetPBiasBuffers, put = SetPBiasBuffers)) std::vector<IntRange>& PBiasBuffers;

private:
    std::vector<std::vector<std::vector<IntRange>>> pWeightBuffers;
public:
    const std::vector<std::vector<std::vector<IntRange>>>& GetPWeightBuffers() const { return pWeightBuffers; }
    void SetPWeightBuffers(const std::vector<std::vector<std::vector<IntRange>>>& value) { pWeightBuffers = value; }
    __declspec(property(get = GetPWeightBuffers, put = SetPWeightBuffers)) std::vector<std::vector<std::vector<IntRange>>>& PWeightBuffers;

private:
    std::vector<IntRange> prevPBiasBuffers;
public:
    const std::vector<IntRange>& GetPrevPBiasBuffers() const { return prevPBiasBuffers; }
    void SetPrevPBiasBuffers(const std::vector<IntRange>& value) { prevPBiasBuffers = value; }
    __declspec(property(get = GetPrevPBiasBuffers, put = SetPrevPBiasBuffers)) std::vector<IntRange>& PrevPBiasBuffers;

private:
    std::vector<std::vector<std::vector<IntRange>>> prevPWeightBuffers;
public:
    const std::vector<std::vector<std::vector<IntRange>>>& GetPrevPWeightBuffers() const { return prevPWeightBuffers; }
    void SetPrevPWeightBuffers(const std::vector<std::vector<std::vector<IntRange>>>& value) { prevPWeightBuffers = value; }
    __declspec(property(get = GetPrevPWeightBuffers, put = SetPrevPWeightBuffers)) std::vector<std::vector<std::vector<IntRange>>>& PrevPWeightBuffers;

private:
    boost::optional<IntRange> netDerivBuffer;
public:
    const boost::optional<IntRange>& GetNetDerivBuffer() const { return netDerivBuffer; }
    void SetNetDerivBuffer(const boost::optional<IntRange>& value) { netDerivBuffer = value; }
    __declspec(property(get = GetNetDerivBuffer, put = SetNetDerivBuffer)) boost::optional<IntRange>& NetDerivBuffer;

private:
    boost::optional<int> biasGradientValueIndex;
public:
    const boost::optional<int>& GetBiasGradientValueIndex() const { return biasGradientValueIndex; }
    void SetBiasGradientValueIndex(const boost::optional<int>& value) { biasGradientValueIndex = value; }
    __declspec(property(get = GetBiasGradientValueIndex, put = SetBiasGradientValueIndex)) boost::optional<int>& BiasGradientValueIndex;

private:
    boost::optional<int> biasGradientSumValueIndex;
public:
    const boost::optional<int>& GetBiasGradientSumValueIndex() const { return biasGradientSumValueIndex; }
    void SetBiasGradientSumValueIndex(const boost::optional<int>& value) { biasGradientSumValueIndex = value; }
    __declspec(property(get = GetBiasGradientSumValueIndex, put = SetBiasGradientSumValueIndex)) boost::optional<int>& BiasGradientSumValueIndex;

private:
    std::vector<IntRange> gradientBuffers;
public:
    const std::vector<IntRange>& GetGradientBuffers() const { return gradientBuffers; }
    void SetGradientBuffers(const std::vector<IntRange>& value) { gradientBuffers = value; }
    __declspec(property(get = GetGradientBuffers, put = SetGradientBuffers)) std::vector<IntRange>& GradientBuffers;

private:
    std::vector<IntRange> gradientSumBuffers;
public:
    const std::vector<IntRange>& GetGradientSumBuffers() const { return gradientSumBuffers; }
    void SetGradientSumBuffers(const std::vector<IntRange>& value) { gradientSumBuffers = value; }
    __declspec(property(get = GetGradientSumBuffers, put = SetGradientSumBuffers)) std::vector<IntRange>& GradientSumBuffers;

private:
    boost::optional<IntRange> outputErrorBuffer;
public:
    const boost::optional<IntRange>& GetOutputErrorBuffer() const { return outputErrorBuffer; }
    void SetOutputErrorBuffer(const boost::optional<IntRange>& value) { outputErrorBuffer = value; }
    __declspec(property(get = GetOutputErrorBuffer, put = SetOutputErrorBuffer)) boost::optional<IntRange>& OutputErrorBuffer;

#pragma endregion

public:
    virtual void Compute(void* valueBuffer, bool collectTrainingData, int innerIterationIndex) { }

    void Reset(CPPAmpValueBuffer& buffer, ::NeuralNetworkResetTarget target);
};
