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
    enum ActivationKernelVersion
    {
        SigmoidAKV = 1 << 1,
        LinearAKV = 1 << 2,
        RTLRAKV = 1 << 3
    };

    enum ComputeGradientsKernelVersion
    {
        FFOnlineCGKV,
        FFOfflineCGKV,
        FFOnlineOfflineCGKV,
        BPTTPhase1CGKV,
        BPTTPhase2CGKV,
        BPTTPhase2OfflineCGKV
    };

    enum ComputeOutputErrorKernelVersion
    {
        SigmoidCOKV,
        LinearCOKV
    };

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
    class OCLProgramUnit;
    class OCLProgram;
    class OCLVault;

    typedef std::shared_ptr<OCLIntCtx> OCLIntCtxSPtrT;

    typedef std::shared_ptr<OCLProgramUnit> OCLProgramUnitSPtrT;
    typedef std::shared_ptr<OCLProgram> OCLProgramSPtrT;
    typedef std::shared_ptr<OCLVault> OCLVaultSPtrT;

    typedef std::vector<OCLKernelToExecute> OCLKernelToExecuteVecT;

    typedef std::vector<std::function<std::reference_wrapper<const OCLBuffer1>()>> OCLBufferRefFactoryVecT;

    typedef std::vector<std::reference_wrapper<const OCLBuffer1>> OCLBuffer1RefVecT;

    typedef std::vector<std::reference_wrapper<const OCLBuffer2>> OCLBuffer2RefVecT;

    typedef std::shared_ptr<OCLBuffer1> OCLBuffer1SPtrT;

    typedef std::shared_ptr<OCLBuffer2> OCLBuffer2SPtrT;
}