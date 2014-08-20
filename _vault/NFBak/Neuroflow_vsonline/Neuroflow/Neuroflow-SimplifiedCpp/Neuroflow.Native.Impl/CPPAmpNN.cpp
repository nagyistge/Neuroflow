#include "stdafx.h"
#include "CPPAmpNN.h"
#include "CPPAmpValueBuffer.h"
#include "CPPAmpVectorBuffer.h"

using namespace concurrency;

CPPAmpNN::CPPAmpNN() 
{
}

Disposable* CPPAmpNN::CreateContext(int size)
{
    return new CPPAmpValueBuffer(accelerator(accelerator::default_accelerator).default_view, size, inputBuffer, outputBuffer);
}

IVectorBuffer* CPPAmpNN::CreateVectorBuffer()
{
    return new CPPAmpVectorBuffer(accelerator(accelerator::default_accelerator).default_view);
}

void CPPAmpNN::ResetAll(Disposable* buffer)
{
    ((CPPAmpValueBuffer*)buffer)->ZeroAll();
}

void CPPAmpNN::ResetForwardValues(Disposable* buffer, NeuralNetworkResetTarget target)
{
    auto buff = ((CPPAmpValueBuffer*)buffer);

    buff->Zero(inputBuffer);

    for (auto git = forwardComputeGroups.begin(); git != forwardComputeGroups.end(); git++)
    {
        for (auto cit = (*git).begin(); cit != (*git).end(); cit++)
        {
            (*cit)->Reset(*buff, target);
        }
    }

    buff->Fill(outputBuffer, 555.555f);
}

void CPPAmpNN::WriteInput(Disposable* buffer, IVectorBuffer* vectorBuff, const VectorHandle& input)
{
    ((CPPAmpValueBuffer*)buffer)->WriteInput(GetArray(vectorBuff, input));
}

void CPPAmpNN::ReadOutput(Disposable* buffer, float* output)
{
    ((CPPAmpValueBuffer*)buffer)->ReadOutput(output);
}

array<float>& CPPAmpNN::GetArray(IVectorBuffer* vectorBuff, const VectorHandle& output)
{
    return ((CPPAmpVectorBuffer*)vectorBuff)->GetArray(output); //TODO: Verify type
}