#include "stdafx.h"
#include "OCLVectorUtils.h"
#include "OCLProgramBuilder.h"
#include "OCLIntCtx.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"
#include "OCLDataArray.h"
#include "GetVectorSize.h"
#include "OCLError.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLVectorKernelName OCLVectorUtils::AddMSEName = OCLVectorKernelName("AddMSE");
OCLVectorKernelName OCLVectorUtils::DivName = OCLVectorKernelName("Div");
OCLVectorKernelName OCLVectorUtils::ZeroFName = OCLVectorKernelName("ZeroF");

void OCLVectorUtils::Build(OCLProgramBuilder& program)
{
    // Defines
    program.Add("\n#define D 100000000.0f\n");

    // Common:
    DEFINE_OCL_PROGRAM(program,

    inline void AtomAdd(__local int* ptr, int v)
    {
        atom_add(ptr, v);
    }

    typedef struct
    {
        union
        {
            int2 vecInt;
            int ints[2];
        };
    } Int2CastType;
    
    inline void AtomAdd2(__local int2* ptr, int2 v)
    {
        __local int* array = ((__local Int2CastType*)ptr)->ints;
        atom_add(&(array[0]), v.s0);
        atom_add(&(array[1]), v.s1);
    }
    
    typedef struct
    {
        union
        {
            int4 vecInt;
            int ints[4];
        };
    } Int4CastType;
    
    inline void AtomAdd4(__local int4* ptr, int4 v)
    {
        __local int* array = ((__local Int4CastType*)ptr)->ints;
        atom_add(&(array[0]), v.s0);
        atom_add(&(array[1]), v.s1);
        atom_add(&(array[2]), v.s2);
        atom_add(&(array[3]), v.s3);
    }
    
    typedef struct
    {
        union
        {
            int8 vecInt;
            int ints[8];
        };
    } Int8CastType;
    
    inline void AtomAdd8(__local int8* ptr, int8 v)
    {
        __local int* array = ((__local Int8CastType*)ptr)->ints;
        atom_add(&(array[0]), v.s0);
        atom_add(&(array[1]), v.s1);
        atom_add(&(array[2]), v.s2);
        atom_add(&(array[3]), v.s3);
        atom_add(&(array[4]), v.s4);
        atom_add(&(array[5]), v.s5);
        atom_add(&(array[6]), v.s6);
        atom_add(&(array[7]), v.s7);
    }
    
    typedef struct
    {
        union
        {
            int16 vecInt;
            int ints[16];
        };
    } Int16CastType;
    
    inline void AtomAdd16(__local int16* ptr, int16 v)
    {
        __local int* array = ((__local Int16CastType*)ptr)->ints;
        atom_add(&(array[0]), v.s0);
        atom_add(&(array[1]), v.s1);
        atom_add(&(array[2]), v.s2);
        atom_add(&(array[3]), v.s3);
        atom_add(&(array[4]), v.s4);
        atom_add(&(array[5]), v.s5);
        atom_add(&(array[6]), v.s6);
        atom_add(&(array[7]), v.s7);
        atom_add(&(array[8]), v.s8);
        atom_add(&(array[9]), v.s9);
        atom_add(&(array[10]), v.sa);
        atom_add(&(array[11]), v.sb);
        atom_add(&(array[12]), v.sc);
        atom_add(&(array[13]), v.sd);
        atom_add(&(array[14]), v.se);
        atom_add(&(array[15]), v.sf);
    }

    inline float SumComponents(float value)
    {
        return value;
    }

    inline float SumComponents2(float2 value)
    {
        return value.x + value.y;
    }

    inline float SumComponents4(float4 value)
    {
        return value.s0 + value.s1 + value.s2 + value.s3;
    }

    inline float SumComponents8(float8 value)
    {
        return SumComponents4(value.lo) + SumComponents4(value.hi);
    }

    inline float SumComponents16(float16 value)
    {
        return SumComponents8(value.lo) + SumComponents8(value.hi);
    }

    inline int GetIndex2(int i1, int i2, int size1)
    {
        return i2 * size1 + i1;
    }

    );


    // Zero
    DEFINE_OCL_PROGRAM(program,

    __kernel void ZeroF$(__global float$* buffer)
    {
        buffer[get_global_id(0)] = (float$)(0.0f);
    }

    );

    // Auto Vec Functions:
    DEFINE_OCL_PROGRAM(program,

    __kernel void AddMSE$(__global float$* desiredValues, __global float$* currentValues, unsigned valueCount, __global float* mseValues, unsigned mseValueIndex)
    {
        float$ mse = 0.0f;
        for (int x = 0; x < valueCount; x++)
        {
            float$ error = (desiredValues[x] - currentValues[x]) * 0.5f;
            mse += error * error;
        }
        mseValues[mseValueIndex] += SumComponents$(mse) / (float)(valueCount * $$);
    }

    __kernel void Div$(__global float$* values, float byValue)
    {
        values[get_global_id(0)] /= byValue;
    }

    );
}

void OCLVectorUtils::AddMSE(const OCLBuffer1& desiredValues, const OCLBuffer1& currentValues, const OCLBuffer1& mseValues, unsigned mseValueIndex)
{
    assert(desiredValues.GetSize() == currentValues.GetSize());
    assert(mseValueIndex < mseValues.GetSize());

    unsigned vectorSize = GetVectorSize(cref(desiredValues));

    addExec.Execute(
        ctx,
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
        ctx,
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
        /*ctx->GetQueue().enqueueFillBuffer(
            buff.GetCLBuffer(),
            0.0f,
            0,
            sizeof(float) * buff.GetSize());*/
        auto vectorSize = GetVectorSize(cref(buff));
        zeroFExec.Execute(
            ctx,
            ZeroFName(vectorSize),
            vectorSize,
            [&](Kernel& kernel)
            {
                kernel.setArg(0, buff.GetCLBuffer());
            },
            buff.GetSize() / vectorSize);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}