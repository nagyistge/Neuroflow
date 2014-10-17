#pragma once

#include "OCL.h"
#include "OCLTypedefs.h"
#include <vector>

namespace NeuroflowN
{
    class OCLOutOfOrderQueue
    {
        OCLIntCtx* ctx;
        std::vector<cl::CommandQueue> queues;
        bool emulated;
        unsigned launchIndex = 0;

        void IncLaunchIndex()
        {
            launchIndex++;
            if (launchIndex >= queues.size()) launchIndex = 0;
        }

    public:
        OCLOutOfOrderQueue(OCLIntCtx* ctx);

        void EnqueueTask(const cl::Kernel kernel);
        void EnqueueNDRangeKernel(const cl::Kernel kernel, const cl::NDRange& workItemOffsets, const cl::NDRange& workItemSizes, const cl::NDRange& localSizes);
        void Begin();
        void End();
    };
}