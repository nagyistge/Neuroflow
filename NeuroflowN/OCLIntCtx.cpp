#include "stdafx.h"
#include "OCLIntCtx.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include "OCLDataArray.h"
#include "OCLKernelToExecute.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLIntCtx::OCLIntCtx(cl::Context context, cl::Device device, cl::Program program, cl::CommandQueue queue) :
    context(context),
    device(device),
    queue(queue),
    program(program),
    isCPU((device.getInfo<CL_DEVICE_TYPE>() & CL_DEVICE_TYPE_CPU) != 0),
    //isCPU(false),
    maxComputeUnits(device.getInfo<CL_DEVICE_MAX_COMPUTE_UNITS>()),
    maxWorkGroupSize(device.getInfo<CL_DEVICE_MAX_WORK_GROUP_SIZE>()),
    maxWorkItemSizes(cl::NullRange)
{
    auto sizes = device.getInfo<CL_DEVICE_MAX_WORK_ITEM_SIZES>();
    maxWorkItemSizes = cl::NDRange(sizes[0], sizes[1], sizes[2]);
}

const OCLBuffer1& OCLIntCtx::ToBuffer1(IDeviceArray* a)
{
    switch (a->GetType())
    {
    case DeviceArrayType::DeviceArray:
        return *((OCLBuffer1*)a);
    case DeviceArrayType::DeviceArray2:
        return ((OCLBuffer2*)a)->GetBaseBufferCRef();
    default:
        return ((OCLDataArray*)a)->GetBuffer();
    }
}

const OCLBuffer2& OCLIntCtx::ToBuffer2(IDeviceArray* a)
{
    auto pb = dynamic_cast<OCLBuffer2*>(a);
    if (pb != null) return *pb;
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
    if (size < maxWorkItemSizes[0]) return size;
    return maxWorkItemSizes[0];
}