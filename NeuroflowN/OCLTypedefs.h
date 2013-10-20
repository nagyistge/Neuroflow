#pragma once 

#include "Typedefs.h"
#include <memory>
#include <array>

namespace cl
{
    class Context;
    class Device;
    class Buffer;
    class Program;
}

namespace NeuroflowN
{
    struct OCLReduceSizes;

    class OCLContextImpl;
    class OCLIntCtx;
    class OCLBuffer1;
    class OCLBuffer2;
    class OCLProgramBuilder;
    class OCLDataArray;
    class OCLDataArrayFactory;
    class OCLKernelToExecute;
    class OCLVectorUtils;
    class OCLMultilayerPerceptronAdapter;
    class OCLDeviceArrayManagement;

    typedef std::shared_ptr<OCLIntCtx> OCLIntCtxSPtrT;

    typedef std::vector<OCLKernelToExecute> OCLKernelToExecuteVecT;

    typedef std::vector<std::function<std::reference_wrapper<const OCLBuffer1>()>> OCLBufferRefFactoryVecT;

    typedef std::vector<std::reference_wrapper<const OCLBuffer1>> OCLBuffer1RefVecT;

    typedef std::vector<std::reference_wrapper<const OCLBuffer2>> OCLBuffer2RefVecT;

    typedef std::shared_ptr<OCLBuffer1> OCLBuffer1SPtrT;

    typedef std::shared_ptr<OCLBuffer2> OCLBuffer2SPtrT;
}