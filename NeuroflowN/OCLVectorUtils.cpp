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

void OCLVectorUtils::Build(const OCLVaultSPtrT& vault)
{
    program = make_shared<OCLProgram>(ctx, "VectorUtilsPrg");
    program->Using(vault->GetCommonCode());

    // Zero
    ADD_OCL_CODE(program,

    kernel void ZeroF$(global float$* buffer, unsigned limit)
    {
        int idx = get_global_id(0);
        int gs = get_global_size(0);

        while (idx < limit)
        {
            buffer[idx] = (float$)(0.0f);

            idx += gs;
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

void OCLVectorUtils::AddMSE(const OCLBuffer1& desiredValues, const OCLBuffer1& currentValues, const OCLBuffer1& mseValues, unsigned mseValueIndex)
{
    assert(desiredValues.GetSize() == currentValues.GetSize());
    assert(mseValueIndex < mseValues.GetSize());

    unsigned vectorSize = GetVectorSize(cref(desiredValues));

    addExec.Execute(
        program,
        AddMSEName(vectorSize),
        vectorSize,
        [&, vectorSize, mseValueIndex](Kernel& kernel)
        {
            kernel.setArg(0, desiredValues.GetCLBuffer());
            kernel.setArg(1, currentValues.GetCLBuffer());
            kernel.setArg(2, desiredValues.GetSize() / vectorSize);
            kernel.setArg(3, mseValues.GetCLBuffer());
            kernel.setArg(4, mseValueIndex);
        },
        1);
}

void OCLVectorUtils::Div(const OCLBuffer1& values, unsigned valueIndex, float byValue)
{
    assert(valueIndex < values.GetSize());

    divExec.Execute(
        program,
        DivName(1),
        1,
        [&, valueIndex, byValue](Kernel& kernel)
        {
            kernel.setArg(0, values.GetCLBuffer());
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
        auto& buffer = ctx->ToBuffer1(values);

        uniform_real_distribution<float> uniform_distribution(min, max);
        auto randF = bind(uniform_distribution, ref(generator));
        vector<float> v;
        v.reserve(buffer.GetSize());
        for (unsigned i = 0; i < buffer.GetSize(); i++) v.push_back(randF());

        ctx->GetQueue().enqueueWriteBuffer(
            buffer.GetCLBuffer(),
            true,
            0,
            buffer.GetSize() * sizeof(float) ,
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
        auto mseA = dynamic_cast<const OCLDataArray*>(mseValues);

        verify_arg(mseA != nullptr, "mseValues");
        verify_arg(valueIndex >= 0 && valueIndex < mseA->GetSize(), "valueIndex");

        ctx->GetQueue().enqueueFillBuffer<float>(mseA->GetBuffer().GetCLBuffer(), 0.0f, valueIndex * sizeof(float) , sizeof(float) );

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
                        oclDesired->GetBuffer(),
                        oclActual->GetBuffer(),
                        mseA->GetBuffer(),
                        valueIndex);

                    count++;
                }
            }
        }

        if (count != 0.0f)
        {
            Div(mseA->GetBuffer(), valueIndex, count);
        }
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void OCLVectorUtils::Zero(IDeviceArray* deviceArray)
{
    try
    {
        auto& buff = ctx->ToBuffer1(deviceArray);
        auto vectorSize = GetVectorSize(cref(buff));
        auto size = buff.GetSize() / vectorSize;
        zeroFExec.Execute(
            program,
            ZeroFName(vectorSize),
            vectorSize,
            [&](Kernel& kernel)
            {
                kernel.setArg(0, buff.GetCLBuffer());
                kernel.setArg(1, size);
            },
            size);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}