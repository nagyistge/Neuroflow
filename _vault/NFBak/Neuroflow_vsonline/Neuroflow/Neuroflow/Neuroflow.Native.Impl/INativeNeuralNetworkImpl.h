#pragma once

#include "Typedefs.h"
#include "Enums.h"
#include "Disposable.h"
#include "IVectorBuffer.h"

__interface INativeNeuralNetworkImpl
{
public:
    virtual void StoreLayerForwardComputeGroups(LayerForwardComputeGroups&& groups) = 0;

    virtual void InitializeInputAndOutput(const IntRange& ib, const IntRange& ob) = 0;

    virtual Disposable* CreateContext(int size) = 0;

    virtual IVectorBuffer* CreateVectorBuffer() = 0;

    virtual void ResetAll(Disposable* buffer) = 0;

    virtual void ResetForwardValues(Disposable* buffer, NeuralNetworkResetTarget target) = 0;

    virtual void WriteInput(Disposable* buffer, IVectorBuffer* vectorBuff, const VectorHandle& input) = 0;

    virtual void ReadOutput(Disposable* buffer, float* output) = 0;
};

