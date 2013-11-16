#include "stdafx.h"
#include "OCLVectorUtils.h"
#include "OCLProgram.h"
#include "OCLIntCtx.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include "OCLDataArray.h"
#include "GetVectorSize.h"
#include "OCLError.h"
#include "OCLVault.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLVectorKernelName OCLVectorUtils::AddMSEName = OCLVectorKernelName("AddMSE");
OCLVectorKernelName OCLVectorUtils::DivName = OCLVectorKernelName("Div");
OCLVectorKernelName OCLVectorUtils::ZeroFName = OCLVectorKernelName("ZeroF");

OCLVectorUtils::OCLVectorUtils(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault) :
    ctx(ctx),
    generator((std::random_device()() << 16) | std::random_device()())
{
    Build(vault);

    z2.s[0] = 0.0f;
    z2.s[1] = 0.0f;

    z4.s[0] = 0.0f;
    z4.s[1] = 0.0f;
    z4.s[2] = 0.0f;
    z4.s[3] = 0.0f;

    z8.s[0] = 0.0f;
    z8.s[1] = 0.0f;
    z8.s[2] = 0.0f;
    z8.s[3] = 0.0f;
    z8.s[4] = 0.0f;
    z8.s[5] = 0.0f;
    z8.s[6] = 0.0f;
    z8.s[7] = 0.0f;

    z16.s[0] = 0.0f;
    z16.s[1] = 0.0f;
    z16.s[2] = 0.0f;
    z16.s[3] = 0.0f;
    z16.s[4] = 0.0f;
    z16.s[5] = 0.0f;
    z16.s[6] = 0.0f;
    z16.s[7] = 0.0f;
    z16.s[8] = 0.0f;
    z16.s[9] = 0.0f;
    z16.s[10] = 0.0f;
    z16.s[11] = 0.0f;
    z16.s[12] = 0.0f;
    z16.s[13] = 0.0f;
    z16.s[14] = 0.0f;
    z16.s[15] = 0.0f;
}

void OCLVectorUtils::Build(const OCLVaultSPtrT& vault)
{
    program = make_shared<OCLProgram>(ctx, "VectorUtilsPrg");
    program->Using(vault->GetCommonCode());

    // Zero
    ADD_OCL_CODE(program,

    kernel void ZeroF$(global float$* buffer, int size)
    {
        int block = size / get_global_size(0) + (size % get_global_size(0) != 0 ? 1 : 0);
        int idx = get_global_id(0) * block;
        int max = idx + block;
        if (max > size) max = size;
        while (idx <  max)
        {
            buffer[idx] = 0.0f;
            idx++;
        }
    }

    );

    // Auto Vec Functions:
    ADD_OCL_CODE(program,

    kernel void AddMSE$(global float$* desiredValues, global float$* currentValues, unsigned valueCount, global float* mseValues, unsigned mseValueIndex)
    {
        float$ mse = 0.0f;
        for (int x = 0; x < valueCount; x++)
        {
            float$ error = (desiredValues[x] - currentValues[x]) * 0.5f;
            mse += error * error;
        }
        mseValues[mseValueIndex] += SumComponents$(mse) / (float)(valueCount * $$);
    }

    kernel void Div$(global float$* values, float byValue)
    {
        values[get_global_id(0)] /= byValue;
    }

    );
}

unsigned OCLVectorUtils::GetPreferredWorkgroupSizeMul()
{
    auto k = program->CreateKernel(ZeroFName(1));
    return (unsigned)k.getWorkGroupInfo<CL_KERNEL_PREFERRED_WORK_GROUP_SIZE_MULTIPLE>(ctx->GetDevice());
}

void OCLVectorUtils::AddMSE(OCLBuffer1* desiredValues, OCLBuffer1* currentValues, OCLBuffer1* mseValues, unsigned mseValueIndex)
{
    assert(desiredValues->GetSize() == currentValues->GetSize());
    assert(mseValueIndex < mseValues->GetSize());

    unsigned vectorSize = GetVectorSize(desiredValues);

    addExec.Execute(
        program,
        AddMSEName(vectorSize),
        vectorSize,
        [&, vectorSize, mseValueIndex](Kernel& kernel)
        {
        kernel.setArg(0, desiredValues->GetCLBuffer());
        kernel.setArg(1, currentValues->GetCLBuffer());
        kernel.setArg(2, desiredValues->GetSize() / vectorSize);
        kernel.setArg(3, mseValues->GetCLBuffer());
        kernel.setArg(4, mseValueIndex);
        },
        1);
}

void OCLVectorUtils::Div(OCLBuffer1* values, unsigned valueIndex, float byValue)
{
    assert(valueIndex < values->GetSize());

    divExec.Execute(
        program,
        DivName(1),
        1,
        [&, valueIndex, byValue](Kernel& kernel)
        {
            kernel.setArg(0, values->GetCLBuffer());
            kernel.setArg(1, byValue);
        },
        NDRange(valueIndex), 
        NDRange(1),
        NullRange);
}

void OCLVectorUtils::RandomizeUniform(IDeviceArray* values, float min, float max)
{
    try
    {
        auto buffer = ctx->ToBuffer1(values);

        uniform_real_distribution<float> uniform_distribution(min, max);
        auto randF = bind(uniform_distribution, ref(generator));
        vector<float> v;
        v.reserve(buffer->GetSize());
        for (unsigned i = 0; i < buffer->GetSize(); i++) v.push_back(randF());

        ctx->GetQueue().enqueueWriteBuffer(
            buffer->GetCLBuffer(),
            true,
            0,
            buffer->GetSize() * sizeof(float),
            &v[0]);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void OCLVectorUtils::CalculateMSE(const SupervisedBatchT& batch, DataArray* mseValues, unsigned valueIndex)
{
    try
    {
        auto mseA = dynamic_cast<OCLDataArray*>(mseValues);

        verify_arg(mseA != nullptr, "mseValues");
        verify_arg(valueIndex >= 0 && valueIndex < mseA->GetSize(), "valueIndex");

        ctx->GetQueue().enqueueFillBuffer<float>(mseA->GetBaseBuffer()->GetCLBuffer(), 0.0f, valueIndex * sizeof(float), sizeof(float));

        float count = 0.0f;

        for (auto& sample : batch)
        {
            for (auto& entry : sample)
            {
                auto desiredOutputs = get<1>(entry);
                auto actualOutputs = get<2>(entry);
                if (desiredOutputs != null && actualOutputs != null)
                {
                    auto oclDesired = dynamic_cast<OCLDataArray*>(desiredOutputs);
                    if (oclDesired == nullptr) throw_logic_error("Unsupported data array type in 'DesiredOutputs'.");

                    auto oclActual = dynamic_cast<OCLDataArray*>(actualOutputs);
                    if (oclActual == nullptr) throw_logic_error("Unsupported data array type in 'ActualOutputs'.");

                    AddMSE(
                        oclDesired->GetBaseBuffer(),
                        oclActual->GetBaseBuffer(),
                        mseA->GetBaseBuffer(),
                        valueIndex);

                    count++;
                }
            }
        }

        if (count != 0.0f)
        {
            Div(mseA->GetBaseBuffer(), valueIndex, count);
        }
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void OCLVectorUtils::Zero(IDeviceArray* deviceArray)
{
    auto buff = ctx->ToBuffer1(deviceArray);
    Zero(buff->GetCLBuffer(), buff->GetSize());
}

void OCLVectorUtils::Zero(const cl::Buffer buffer, unsigned size)
{
    try
    {
        auto vectorSize = GetVectorSize(size);

        if (ctx->IsCPU())
        {
            switch (vectorSize)
            {
                case 2:
                {
                          ctx->GetQueue().enqueueFillBuffer(
                              buffer,
                              z2,
                              0,
                              sizeof(float)* size);
                }
                    break;
                case 4:
                {
                          ctx->GetQueue().enqueueFillBuffer(
                              buffer,
                              z4,
                              0,
                              sizeof(float)* size);
                }
                    break;
                case 8:
                {
                          ctx->GetQueue().enqueueFillBuffer(
                              buffer,
                              z8,
                              0,
                              sizeof(float)* size);
                }
                    break;
                case 16:
                {
                           ctx->GetQueue().enqueueFillBuffer(
                               buffer,
                               z16,
                               0,
                               sizeof(float)* size);
                }
                    break;
                default:
                    ctx->GetQueue().enqueueFillBuffer(
                        buffer,
                        0.0f,
                        0,
                        sizeof(float)* size);
                    break;
            }
        }
        else
        {
            zeroFExec.Execute(
                program,
                ZeroFName(vectorSize),
                vectorSize,
                [&](Kernel& kernel)
            {
                kernel.setArg(0, buffer);
                kernel.setArg(1, size / vectorSize);
            },
                ctx->GetOptimalGlobalSize(size, vectorSize));
        }
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}