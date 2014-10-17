#pragma once

#include "OCL.h"
#include "Registry.h"
#include "OCLTypedefs.h"
#include "OCLStructs.h"
#include "NfObject.h"
#include "DeviceInfo.h"

namespace NeuroflowN
{
    class OCLIntCtx
    {
        friend class OCLContextImpl;

        cl::CommandQueue queue;
        cl::Device device;
        cl::Context context;
        DeviceInfo deviceInfo;
        std::string version;
        OCLOutOfOrderQueueSPtrT ooQueue;

        bool isCPU;
        unsigned maxWorkGroupSize;
        unsigned maxComputeUnits;
        unsigned preferredWorkgroupSizeMul;
        unsigned alignBits;
        cl::NDRange maxWorkItemSizes;

        Registry<std::pair<unsigned, unsigned>, std::pair<unsigned, unsigned>> ioReduceSizes;

        Registry<unsigned, std::pair<unsigned, unsigned>> reduceSizes;

    public:
        OCLIntCtx(const cl::Device& device, const DeviceInfo& deviceInfo, const std::string version);

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

        const DeviceInfo& GetDeviceInfo() const
        {
            return deviceInfo;
        }

        const std::string& GetVersion() const
        {
            return version;
        }

        const OCLOutOfOrderQueueSPtrT& GetOutOfOrderQueue();

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

        unsigned GetMaxLayerCount() const
        {
            return 4;
        }

        unsigned GetPreferredWorkgroupSizeMul() const
        {
            return preferredWorkgroupSizeMul;
        }

        const cl::NDRange GetMaxWorkItemSizes() const
        {
            return maxWorkItemSizes;
        }

        unsigned GetAlignBits() const
        {
            return alignBits;
        }

        void Flush()
        {
            queue.flush();
        }

        OCLBuffer1* ToBuffer1(IDeviceArray* a);

        OCLBuffer2* ToBuffer2(IDeviceArray* a);

        std::string AsVectorKernelName(char* kernelName, unsigned vectorSize);

        unsigned GetOptimalGlobalSize(unsigned workItemCount, unsigned vectorSize);

        unsigned GetOptimalLocalSizeForOneWorkgroup(unsigned workItemCount, unsigned vectorSize);

        std::pair<unsigned, unsigned> GetIOReduceSizesInput(unsigned inputSize, unsigned vectorSize, unsigned outputSize);

        std::pair<unsigned, unsigned> GetIOReduceSizesOutput(unsigned inputSize, unsigned outputSize, unsigned vectorSize);

    private:
        inline unsigned GetBestLocalSize(unsigned size);
        unsigned ToPowerOfTwo(unsigned value);

        bool IsPowerOfTwo(unsigned value)
        {
            return (value != 0) && ((value & (value - 1)) == 0);
        }
    };
}