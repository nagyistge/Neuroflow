#include "stdafx.h"
#include "OCLOutOfOrderQueue.h"
#include "OCLIntCtx.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLOutOfOrderQueue::OCLOutOfOrderQueue(OCLIntCtx* ctx) :
ctx(ctx)
{
    auto cap = ctx->GetDevice().getInfo<CL_DEVICE_QUEUE_PROPERTIES>();
    if (cap & CL_QUEUE_OUT_OF_ORDER_EXEC_MODE_ENABLE)
    {
        emulated = false;
        queues.emplace_back(ctx->GetContext(), ctx->GetDevice(), CL_QUEUE_OUT_OF_ORDER_EXEC_MODE_ENABLE);
    }
    else
    {
        emulated = true;
        for (unsigned i = 0; i < ctx->GetMaxComputeUnits(); i++)
        {
            queues.emplace_back(ctx->GetContext(), ctx->GetDevice(), 0);
        }
    }
}

void OCLOutOfOrderQueue::EnqueueTask(const cl::Kernel kernel)
{
    if (emulated)
    {
        queues[launchIndex].enqueueTask(kernel);
        IncLaunchIndex();
    }
    else
    {
        queues[0].enqueueTask(kernel);
    }
}

void OCLOutOfOrderQueue::EnqueueNDRangeKernel(const cl::Kernel kernel, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes)
{
    if (emulated)
    {
        queues[launchIndex].enqueueNDRangeKernel(kernel, workItemOffsets, workItemSizes, localSizes);
        IncLaunchIndex();
    }
    else
    {
        queues[0].enqueueNDRangeKernel(kernel, workItemOffsets, workItemSizes, localSizes);
    }
}

void OCLOutOfOrderQueue::Begin()
{
    Event e;
    clEnqueueBarrierWithWaitList(ctx->GetQueue()(), 0, null, &(e()));
    if (emulated)
    {
        for (unsigned i = 0; i < queues.size(); i++)
        {
            clEnqueueBarrierWithWaitList(queues[i](), 1, &(e()), null);
        }
    }
    else
    {
        clEnqueueBarrierWithWaitList(queues[0](), 1, &(e()), null);
    }
}

void OCLOutOfOrderQueue::End()
{
    if (emulated)
    {
        vector<Event> events(queues.size());
        for (unsigned i = 0; i < queues.size(); i++)
        {
            clEnqueueBarrierWithWaitList(queues[0](), 0, null, (cl_event*)&events.front() + i);
        }
        clEnqueueBarrierWithWaitList(ctx->GetQueue()(), events.size(), (cl_event*)&events.front(), null);
    }
    else
    {
        Event e;
        clEnqueueBarrierWithWaitList(queues[0](), 0, null, &(e()));
        clEnqueueBarrierWithWaitList(ctx->GetQueue()(), 1, &(e()), null);
    }
}
