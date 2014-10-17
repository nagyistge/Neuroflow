#include "stdafx.h"
#include "OCLKernelBase.h"
#include "GetVectorSize.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include "OCLIntCtx.h"

using namespace std;
using namespace NeuroflowN;

OCLKernelBase::OCLKernelBase(const OCLIntCtxSPtrT& ctx) :
    ctx(ctx)
{
}

unsigned OCLKernelBase::CalculateVectorSize(DeviceArrayFVecT* inputList) const
{
    unsigned vectorSize = 16;
    for (unsigned i = 0; i < inputList->size(); i++) 
    {
        auto& f = (*inputList)[i];
        vectorSize = GetVectorSize(vectorSize, cref(ctx->ToBuffer1(f())));
    }
    return vectorSize;
}

std::pair<unsigned, unsigned> OCLKernelBase::GetIOReduceSizesInput(DeviceArrayFVecT* inputList, const OCLBuffer1& outputs, unsigned vectorSize) const
{
    auto sizes = make_pair(0U, 0U);
    for (unsigned i = 0; i < inputList->size(); i++)
    {
        auto& f = (*inputList)[i];
        auto& lowerErrorsI = ctx->ToBuffer1(f());
        auto csizes = ctx->GetIOReduceSizesInput(lowerErrorsI.GetSize(), vectorSize, outputs.GetSize());
        if (csizes.first > sizes.first) sizes = csizes;
    }
    return move(sizes);
}

std::pair<unsigned, unsigned> OCLKernelBase::GetIOReduceSizesOutput(DeviceArrayVecT* inputList, const OCLBuffer1& outputs, unsigned vectorSize) const
{
    auto sizes = make_pair(0U, 0U);
    for (unsigned i = 0; i < inputList->size(); i++)
    {
        auto& inputs = (*inputList)[i];
        auto& lowerErrorsI = ctx->ToBuffer1(inputs);
        auto csizes = ctx->GetIOReduceSizesOutput(lowerErrorsI.GetSize(), outputs.GetSize(), vectorSize);
        if (csizes.first > sizes.first) sizes = csizes;
    }
    return move(sizes);
}