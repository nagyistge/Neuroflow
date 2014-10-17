#include "stdafx.h"
#include "OCLDeviceArrayManagement.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include "OCLError.h"

using namespace NeuroflowN;
using namespace std;
using namespace cl;

IDeviceArray* OCLDeviceArrayManagement::CreateArray(bool copyOptimized, int size)
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
            sizeof(float) * size,
            nullptr);

        ctx->GetQueue().enqueueFillBuffer<float>(
            buffer,
            0.0f, //
            0, // offset
            size * sizeof(float),
            nullptr,
            nullptr);

        return new OCLBuffer1(buffer);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

IDeviceArray2* OCLDeviceArrayManagement::CreateArray2(bool copyOptimized, int rowSize, int colSize)
{
    try
    {
        unsigned size = rowSize * colSize;

        auto buffer = Buffer(
            ctx->GetContext(),
#if _DEBUG
            0,
#else
            copyOptimized ? 0 : CL_MEM_HOST_NO_ACCESS,
#endif
            sizeof(float) * size,
            nullptr);

        ctx->GetQueue().enqueueFillBuffer<float>(
            buffer,
            0.0f, //
            0, // offset
            size * sizeof(float) ,
            nullptr,
            nullptr);

        return new OCLBuffer2(buffer, rowSize);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void OCLDeviceArrayManagement::Copy(IDeviceArray* from, int fromIndex, IDeviceArray* to, int toIndex, int size)
{
    try
    {
        ctx->GetQueue().enqueueCopyBuffer(
            ctx->ToBuffer1(from).GetCLBuffer(),
            ctx->ToBuffer1(to).GetCLBuffer(),
            fromIndex * sizeof(float) ,
            toIndex * sizeof(float) ,
            size * sizeof(float) );
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}