#pragma once

#include "Enums.h"
#include "IntRange.h"
#include "INativeNeuralNetworkImpl.h"
#include "LayerForwardCompute.h"

class CPPAmpNN : public INativeNeuralNetworkImpl
{
private:
    IntRange inputBuffer, outputBuffer;

    LayerForwardComputeGroups forwardComputeGroups;

public:
    CPPAmpNN();

    virtual void StoreLayerForwardComputeGroups(LayerForwardComputeGroups&& groups) override
    {
        forwardComputeGroups = move(groups);
    }

    virtual void InitializeInputAndOutput(const IntRange& ib, const IntRange& ob) override
    {
        inputBuffer = ib;
        outputBuffer = ob;
    }

    virtual Disposable* CreateContext(int size) override;

    virtual IVectorBuffer* CreateVectorBuffer() override;

    virtual void ResetAll(Disposable* buffer) override;

    virtual void ResetForwardValues(Disposable* buffer, NeuralNetworkResetTarget target) override;

    virtual void WriteInput(Disposable* buffer, IVectorBuffer* vectorBuff, const VectorHandle& input) override;

    virtual void ReadOutput(Disposable* buffer, float* output) override;

private:
    concurrency::array<float>& GetArray(IVectorBuffer* vectorBuff, const VectorHandle& output);
};

