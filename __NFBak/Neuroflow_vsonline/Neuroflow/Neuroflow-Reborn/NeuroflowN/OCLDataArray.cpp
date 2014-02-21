#include "stdafx.h"
#include "OCLDataArray.h"
#include "OCLIntCtx.h"
#include "OCLError.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLDataArray::OCLDataArray(const OCLIntCtxSPtrT& ctx, unsigned size, float* values, bool isConst) : 
    ctx(ctx),
    buffer(CreateBuffer(ctx, size, values)),
    isConst(isConst)
{
}

OCLDataArray::OCLDataArray(const OCLIntCtxSPtrT& ctx, unsigned size, float fill) : 
    ctx(ctx),
    buffer(CreateBuffer(ctx, size, fill)),
    isConst(false)
{
}

cl::Buffer OCLDataArray::CreateBuffer(const OCLIntCtxSPtrT& ctx, unsigned size, float* values)
{
	try
	{
        auto flags = CL_MEM_COPY_HOST_PTR;

        if (isConst) flags |= (CL_MEM_HOST_NO_ACCESS | CL_MEM_READ_ONLY);

		return Buffer(
			ctx->GetContext(),
			flags,
			sizeof(float) * size,
			values);
	}
	catch (exception& ex)
	{
		throw as_ocl_error(ex);
	}
}

cl::Buffer OCLDataArray::CreateBuffer(const OCLIntCtxSPtrT& ctx, unsigned size, float fill)
{
	try
	{
		auto buffer = Buffer(
			ctx->GetContext(),
			0,
			sizeof(float) * size,
			nullptr);

		ctx->GetQueue().enqueueFillBuffer<float>(
			buffer,
			fill, //
			0, // offset
			size * sizeof(float),
			nullptr,
			nullptr);

		return buffer;
	}
	catch (exception& ex)
	{
		throw as_ocl_error(ex);
	}
}

void OCLDataArray::Read(int sourceBeginIndex, int count, float* targetPtr, int targetBeginIndex, doneCallback done)
{
    VerifyIsNotConst();

	try
	{
        Event e;

		ctx->GetQueue().enqueueReadBuffer(
			buffer.GetCLBuffer(),
            false, // blocking
			sourceBeginIndex * sizeof(float), // offset
			count * sizeof(float), // size
			targetPtr + targetBeginIndex,
			nullptr,
			&e);
	
		e.setCallback(
			CL_COMPLETE,
			[](cl_event event, cl_int status, void* userData)
			{
                auto cb = (doneCallback*)userData;
				try
				{
					if (status == CL_COMPLETE)
					{
						// Done
						(*cb)(nullptr);
					}
					else
					{
						// Error
						auto error = ocl_error(status, "Cannot read memory.");
						(*cb)(&error);
					}
				}
				catch (...)
				{
					OutputDebugString(L"OCLDataArray::Read done callback cannot be called.\n");
				}
				delete cb;
			},
			new doneCallback(done));

		ctx->GetQueue().flush();
	}
	catch (exception& ex)
	{
		throw as_ocl_error(ex);
	}
}

void OCLDataArray::Write(float* sourceArray, int sourceBeginIndex, int count, int targetBeginIndex, doneCallback done)
{
    VerifyIsNotConst();

	try
	{
        Event e;

		ctx->GetQueue().enqueueWriteBuffer(
			buffer.GetCLBuffer(),
			false, // blocking
			targetBeginIndex * sizeof(float), // offset
			count * sizeof(float), // size
			sourceArray + sourceBeginIndex,
			nullptr,
			&e);
	
		e.setCallback(
			CL_COMPLETE,
			[](cl_event event, cl_int status, void* userData)
			{
                auto cb = (doneCallback*)userData;
				try
				{
					if (status == CL_COMPLETE)
					{
						// Done
						(*cb)(nullptr);
					}
					else
					{
						// Error
						auto error = ocl_error(status, "Cannot read memory.");
						(*cb)(&error);
					}
				}
				catch (...)
				{
					OutputDebugString(L"OCLDataArray::Write done callback cannot be called.\n");
				}
				delete cb;
			},
			new doneCallback(done));

        ctx->GetQueue().flush();
	}
	catch (exception& ex)
	{
		throw as_ocl_error(ex);
	}
}

void OCLDataArray::VerifyIsNotConst()
{
    if (isConst) throw_logic_error("Const data array is not accessible.");
}
