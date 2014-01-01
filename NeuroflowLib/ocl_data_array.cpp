#include "stdafx.h"
#include "ocl_data_array.h"
#include "ocl_computation_context.h"

USING;

ocl_data_array::ocl_data_array(const ocl_computation_context_wptr& context, const cl::Buffer& buffer, bool isConst) :
ocl_device_array(buffer),
ocl_contexted(context),
isConst(isConst)
{
}

bool ocl_data_array::is_const() const
{
    return isConst;
}

concurrency::task<void> ocl_data_array::read(idx_t sourceBeginIndex, idx_t count, float* targetPtr, idx_t targetBeginIndex)
{
    verify_arg(sourceBeginIndex >= 0, "sourceBeginIndex");
    verify_arg(count > 0, "count");
    verify_arg(targetPtr != null, "targetPtr");
    verify_arg(targetBeginIndex >= 0, "targetBeginIndex");

    verify_if_accessible();

    auto ctx = lock_context();
    auto tce = new task_completion_event<void>();
    try
    {
        Event e;
        auto queue = ctx->cl_queue();

        queue.enqueueReadBuffer(
            buffer(),
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
            auto tce = (task_completion_event<void>*)userData;
            try
            {
                if (status == CL_COMPLETE)
                {
                    // Done
                    tce->set();
                }
                else
                {
                    // Error
                    tce->set_exception(ocl_error(status, "Cannot read memory."));
                }
            }
            catch (...)
            {
                OutputDebugString(L"OCLDataArray::Read done callback cannot be called.\n");
            }
            delete tce; 
        },
        tce);

        queue.flush();
    }
    catch (exception& ex)
    {
        delete tce;
        throw as_ocl_error(ex);
    }

    return task<void>(*tce);
}

concurrency::task<void> ocl_data_array::write(float* sourceArray, idx_t sourceBeginIndex, idx_t count, idx_t targetBeginIndex)
{
    verify_arg(sourceBeginIndex >= 0, "sourceBeginIndex");
    verify_arg(count > 0, "count");
    verify_arg(sourceArray != null, "sourceArray");
    verify_arg(targetBeginIndex >= 0, "targetBeginIndex");

    verify_if_accessible();

    auto ctx = lock_context();
    auto tce = new task_completion_event<void>();
    try
    {
        Event e;
        auto queue = ctx->cl_queue();

        queue.enqueueWriteBuffer(
            buffer(),
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
            auto tce = (task_completion_event<void>*)userData;
            try
            {
                if (status == CL_COMPLETE)
                {
                    // Done
                    tce->set();
                }
                else
                {
                    // Error
                    tce->set_exception(ocl_error(status, "Cannot read memory."));
                }
            }
            catch (...)
            {
                OutputDebugString(L"OCLDataArray::Read done callback cannot be called.\n");
            }
            delete tce;
        },
        tce);

        queue.flush();
    }
    catch (exception& ex)
    {
        delete tce;
        throw as_ocl_error(ex);
    }

    return task<void>(*tce);
}

void ocl_data_array::verify_if_accessible() const
{
    if (isConst) throw_runtime_error("Const data array is not accessible.");
}