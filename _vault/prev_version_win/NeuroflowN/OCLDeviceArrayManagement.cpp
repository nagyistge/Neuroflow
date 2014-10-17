#include "stdafx.h"
#include "OCLDeviceArrayManagement.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include "OCLError.h"
#include "OCLDeviceArrayPool.h"

using namespace NeuroflowN;
using namespace std;
using namespace cl;

cl::Buffer OCLDeviceArrayManagement::CreateBuffer(bool copyOptimized, unsigned sizeInBytes)
{
    try
    {
        auto buffer = Buffer(
            ctx->GetContext(),
#if _DEBUG
            0,
#else
            (copyOptimized ? 0 : CL_MEM_HOST_NO_ACCESS),
#endif
            sizeInBytes,
            nullptr);

        ctx->GetQueue().enqueueFillBuffer<float>(
            buffer,
            0.0f, //
            0, // offset
            sizeInBytes,
            nullptr,
            nullptr);

        return buffer;
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

IDeviceArray* OCLDeviceArrayManagement::CreateArray(bool copyOptimized, int size)
{
    return new OCLBuffer1(CreateBuffer(copyOptimized, size * sizeof(float)));
}

IDeviceArray2* OCLDeviceArrayManagement::CreateArray2(bool copyOptimized, int rowSize, int colSize)
{
    return new OCLBuffer2(CreateBuffer(copyOptimized, rowSize * colSize * sizeof(float)), rowSize);
}

void OCLDeviceArrayManagement::Copy(IDeviceArray* from, int fromIndex, IDeviceArray* to, int toIndex, int size)
{
    try
    {
        ctx->GetQueue().enqueueCopyBuffer(
            ctx->ToBuffer1(from)->GetCLBuffer(),
            ctx->ToBuffer1(to)->GetCLBuffer(),
            fromIndex * sizeof(float) ,
            toIndex * sizeof(float) ,
            size * sizeof(float) );
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

IDeviceArrayPool* OCLDeviceArrayManagement::CreatePool()
{
    return new OCLDeviceArrayPool(this, vectorUtils);
}