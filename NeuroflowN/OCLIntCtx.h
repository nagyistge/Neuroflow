#pragma once

#include "OCL.h"
#include "Registry.h"
#include "OCLTypedefs.h"
#include "OCLStructs.h"
#include "NfObject.h"

namespace NeuroflowN
{
    class OCLIntCtx
    {
        cl::CommandQueue queue;
        cl::Program program;
        cl::Device device;
        cl::Context context;

        bool isCPU;
        unsigned maxWorkGroupSize;
        unsigned maxComputeUnits;
        cl::NDRange maxWorkItemSizes;

        Registry<std::pair<unsigned, unsigned>, std::pair<unsigned, unsigned>> ioReduceSizes;

        Registry<unsigned, std::pair<unsigned, unsigned>> reduceSizes;

    public:
        OCLIntCtx(cl::Context context, cl::Device device, cl::Program program, cl::CommandQueue queue);

        const cl::Context& GetContext() const
        {
            return context;
        }

        const cl::Device& GetDevice() const
        {
            return device;
        }

        cl::CommandQueue& GetQueue()
        {
            return queue;
        }

        const cl::Program& GetProgram() const
        {
            return program;
        }

        bool IsCPU() const
        {
            return isCPU;
        }

        unsigned GetMaxComputeUnits() const
        {
            return maxComputeUnits;
        }

        unsigned GetMaxWorkGroupSize() const
        {
            return maxWorkGroupSize;
        }

        unsigned GetMaxConnectionCount() const
        {
            return 4;
        }

        void Flush()
        {
            queue.flush();
        }

        const OCLBuffer1& ToBuffer1(IDeviceArray* a);

        const OCLBuffer2& ToBuffer2(IDeviceArray* a);

        std::string AsVectorKernelName(char* kernelName, unsigned vectorSize);

        std::pair<unsigned, unsigned> GetIOReduceSizesInput(unsigned inputSize, unsigned vectorSize, unsigned outputSize);

        std::pair<unsigned, unsigned> GetIOReduceSizesOutput(unsigned inputSize, unsigned outputSize, unsigned vectorSize);

    private:
        inline unsigned GetBestLocalSize(unsigned size);

        bool IsPowerOfTwo(unsigned value)
        {
            return (value != 0) && ((value & (value - 1)) == 0);
        }
    };
}