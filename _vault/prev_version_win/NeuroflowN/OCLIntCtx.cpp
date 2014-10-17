#include "stdafx.h"
#include "OCLIntCtx.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include "OCLDataArray.h"
#include "OCLKernelToExecute.h"
#include "OCLOutOfOrderQueue.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLIntCtx::OCLIntCtx(
    const cl::Device& device, 
    const DeviceInfo& deviceInfo, 
    const std::string version) :
    context(device),
    device(device),
    deviceInfo(deviceInfo),
    version(version),
    isCPU((device.getInfo<CL_DEVICE_TYPE>() & CL_DEVICE_TYPE_CPU) != 0),
    maxComputeUnits(device.getInfo<CL_DEVICE_MAX_COMPUTE_UNITS>()),
    maxWorkGroupSize(device.getInfo<CL_DEVICE_MAX_WORK_GROUP_SIZE>()),
    maxWorkItemSizes(cl::NullRange),
    alignBits(device.getInfo<CL_DEVICE_MEM_BASE_ADDR_ALIGN>())
{
    queue = CommandQueue(context, device);

    auto sizes = device.getInfo<CL_DEVICE_MAX_WORK_ITEM_SIZES>();
    maxWorkItemSizes = cl::NDRange(sizes[0], sizes[1], sizes[2]);
}

OCLBuffer1* OCLIntCtx::ToBuffer1(IDeviceArray* a)
{
    switch (a->GetType())
    {
    case DeviceArrayType::DeviceArray:
        return ((OCLBuffer1*)a);
    case DeviceArrayType::DeviceArray2:
        return ((OCLBuffer2*)a)->GetBaseBuffer();
    default:
        return ((OCLDataArray*)a)->GetBaseBuffer();
    }
}

OCLBuffer2* OCLIntCtx::ToBuffer2(IDeviceArray* a)
{
    auto pb = dynamic_cast<OCLBuffer2*>(a);
    if (pb != null) return pb;
    throw_logic_error("Device array2 is not for OCL device.");
}

std::string OCLIntCtx::AsVectorKernelName(char* kernelName, unsigned vectorSize)
{
    stringstream ss;
    ss << kernelName;
    if (vectorSize > 1) ss << vectorSize;
    return ss.str();
}

std::pair<unsigned, unsigned> OCLIntCtx::GetIOReduceSizesInput(unsigned inputSize, unsigned vectorSize, unsigned outputSize)
{
    inputSize /= vectorSize;
    return ioReduceSizes.GetOrCreate(
        make_pair(inputSize, outputSize),
        [=]()
    {
        unsigned localSize = GetBestLocalSize(inputSize);
        unsigned workItemsCount = outputSize * localSize;
        return make_pair(workItemsCount, localSize);
    });
}

std::pair<unsigned, unsigned> OCLIntCtx::GetIOReduceSizesOutput(unsigned inputSize, unsigned outputSize, unsigned vectorSize)
{
    outputSize /= vectorSize;
    return ioReduceSizes.GetOrCreate(
        make_pair(inputSize, outputSize),
        [=]()
    {
        unsigned localSize = GetBestLocalSize(inputSize);
        unsigned workItemsCount = outputSize * localSize;
        return make_pair(workItemsCount, localSize);
    });
}

unsigned OCLIntCtx::GetBestLocalSize(unsigned size)
{
    if (size < preferredWorkgroupSizeMul) return ToPowerOfTwo(size);
    unsigned rem = size % preferredWorkgroupSizeMul;
    size -= rem;
    if (size > maxWorkItemSizes[0]) return maxWorkItemSizes[0];
    return ToPowerOfTwo(size);
}

unsigned OCLIntCtx::GetOptimalGlobalSize(unsigned workItemCount, unsigned vectorSize)
{
    unsigned gs = workItemCount / vectorSize;
    if (gs > GetMaxWorkGroupSize())
    {
        gs -= gs % GetMaxWorkGroupSize();
    }
    else if (gs > GetPreferredWorkgroupSizeMul())
    {
        gs -= gs % GetPreferredWorkgroupSizeMul();
    }
    return ToPowerOfTwo(gs);
}

unsigned OCLIntCtx::GetOptimalLocalSizeForOneWorkgroup(unsigned workItemCount, unsigned vectorSize)
{
    unsigned gs = workItemCount / vectorSize;
    if (gs > GetMaxWorkGroupSize())
    {
        gs = GetMaxWorkGroupSize();
    }
    return ToPowerOfTwo(gs);
}

unsigned OCLIntCtx::ToPowerOfTwo(unsigned value)
{
    while (!IsPowerOfTwo(value)) value--;
    return value;
}

const OCLOutOfOrderQueueSPtrT& OCLIntCtx::GetOutOfOrderQueue()
{
    if (ooQueue == null) ooQueue = make_shared<OCLOutOfOrderQueue>(this);
    return ooQueue;
}