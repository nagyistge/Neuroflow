#include "stdafx.h"
#include "OCLDeviceArrayPool.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include "OCLError.h"
#include "OCLDeviceArrayManagement.h"
#include "OCLVectorUtils.h"

using namespace NeuroflowN;
using namespace std;
using namespace cl;

OCLDeviceArrayPool::OCLDeviceArrayPool(OCLDeviceArrayManagement* daMan, const OCLVectorUtilsSPtrT& vectorUtils) : 
daMan(daMan),
vectorUtils(vectorUtils)
{
}

bool OCLDeviceArrayPool::GetIsAllocated() const
{
    return buffer() != null;
}

IDeviceArray* OCLDeviceArrayPool::CreateArray(int size)
{
    return new OCLBuffer1(this, Reserve(size), size);
}

IDeviceArray2* OCLDeviceArrayPool::CreateArray2(int rowSize, int colSize)
{
    return new OCLBuffer2(this, Reserve(rowSize * colSize), rowSize, colSize);
}

void OCLDeviceArrayPool::Allocate()
{
    if (endIndex == 0) throw_logic_error("There is no allocated memory in the pool.");
    if (!GetIsAllocated()) buffer = daMan->CreateBuffer(false, endIndex);
}

void OCLDeviceArrayPool::Zero()
{
    if (!GetIsAllocated()) throw_logic_error("Cannot zero out an unallocated pool.");
    unsigned fsize = endIndex / sizeof(float);
    assert(endIndex % sizeof(float) == 0);
    vectorUtils->Zero(buffer, fsize);
}

int OCLDeviceArrayPool::Reserve(int size)
{
    if (GetIsAllocated()) throw_logic_error("Cannot reserve memory in an already allocated pool.");
    if (endIndex != 0)
    {
        unsigned align = daMan->ctx->GetAlignBits(); // bits
        align /= 8; // bytes
        while (endIndex % align != 0) endIndex++;
    }
    int beginIndex = endIndex;
    endIndex += size * sizeof(float);
    return beginIndex;
}

cl::Buffer OCLDeviceArrayPool::CreateSubBuffer(unsigned beginOffset, unsigned size)
{
    Allocate();
    cl_buffer_region r;
    r.origin = beginOffset;
    r.size = size * sizeof(float);
    return buffer.createSubBuffer(CL_MEM_HOST_NO_ACCESS, CL_BUFFER_CREATE_TYPE_REGION, &r);
}
