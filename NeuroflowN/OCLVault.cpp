#include "stdafx.h"
#include "OCLVault.h"
#include "OCLProgramUnit.h"
#include "OCLIntCtx.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLVault::OCLVault(const OCLIntCtxSPtrT& ctx) :
    ctx(ctx)
{
    // Common:

    commonCode = make_shared<OCLProgramUnit>(ctx, "common.h");
    commonCode->AddCode("#pragma OPENCL EXTENSION cl_khr_local_int32_base_atomics : enable");
    commonCode->AddCode("#define D 100000000.0f");
    commonCode->AddCode("#define null 0");

    ADD_OCL_CODE(commonCode,
    
    inline void AtomAdd(local int* ptr, int v)
    {
        atomic_add(ptr, v);
    }

    typedef struct
    {
        union
        {
            int2 vecInt;
            int ints[2];
        };
    } Int2CastType;

    inline void AtomAdd2(local int2* ptr, int2 v)
    {
        local int* array = ((local Int2CastType*)ptr)->ints;
        atomic_add(&(array[0]), v.s0);
        atomic_add(&(array[1]), v.s1);
    }

    typedef struct
    {
        union
        {
            int4 vecInt;
            int ints[4];
        };
    } Int4CastType;

    inline void AtomAdd4(local int4* ptr, int4 v)
    {
        local int* array = ((local Int4CastType*)ptr)->ints;
        atomic_add(&(array[0]), v.s0);
        atomic_add(&(array[1]), v.s1);
        atomic_add(&(array[2]), v.s2);
        atomic_add(&(array[3]), v.s3);
    }

    typedef struct
    {
        union
        {
            int8 vecInt;
            int ints[8];
        };
    } Int8CastType;

    inline void AtomAdd8(local int8* ptr, int8 v)
    {
        local int* array = ((local Int8CastType*)ptr)->ints;
        atomic_add(&(array[0]), v.s0);
        atomic_add(&(array[1]), v.s1);
        atomic_add(&(array[2]), v.s2);
        atomic_add(&(array[3]), v.s3);
        atomic_add(&(array[4]), v.s4);
        atomic_add(&(array[5]), v.s5);
        atomic_add(&(array[6]), v.s6);
        atomic_add(&(array[7]), v.s7);
    }

    typedef struct
    {
        union
        {
            int16 vecInt;
            int ints[16];
        };
    } Int16CastType;

    inline void AtomAdd16(local int16* ptr, int16 v)
    {
        local int* array = ((local Int16CastType*)ptr)->ints;
        atomic_add(&(array[0]), v.s0);
        atomic_add(&(array[1]), v.s1);
        atomic_add(&(array[2]), v.s2);
        atomic_add(&(array[3]), v.s3);
        atomic_add(&(array[4]), v.s4);
        atomic_add(&(array[5]), v.s5);
        atomic_add(&(array[6]), v.s6);
        atomic_add(&(array[7]), v.s7);
        atomic_add(&(array[8]), v.s8);
        atomic_add(&(array[9]), v.s9);
        atomic_add(&(array[10]), v.sa);
        atomic_add(&(array[11]), v.sb);
        atomic_add(&(array[12]), v.sc);
        atomic_add(&(array[13]), v.sd);
        atomic_add(&(array[14]), v.se);
        atomic_add(&(array[15]), v.sf);
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

    // Net

    netCode = make_shared<OCLProgramUnit>(ctx, "net.h");

    ADD_OCL_CODE(netCode,

    inline float$ Get2$(global float$* values, int i1, int i2, int size1)
    {
        return values[GetIndex2(i1, i2, size1)];
    }

    inline void Set2$(global float$* values, int i1, int i2, int size1, float$ value)
    {
        values[GetIndex2(i1, i2, size1)] = value;
    }

    inline void Add2$(global float$* values, int i1, int i2, int size1, float$ value)
    {
        values[GetIndex2(i1, i2, size1)] += value;
    }

    inline void SetAdd2$(global float$* values1, global float$* values2, int i1, int i2, int size1, float$ value)
    {
        int index = GetIndex2(i1, i2, size1);
        values1[index] = value;
        values2[index] += value;
    }

    inline void AddDiv2$(global float$* values, int i1, int i2, int size1, float$ value, float by)
    {
        int index = GetIndex2(i1, i2, size1);
        values[index] += value;
        values[index] /= by;
    }

    inline void AddDivAdd2$(global float$* values1, global float$* values2, int i1, int i2, int size1, float$ value, float by)
    {
        int index = GetIndex2(i1, i2, size1);
        values1[index] += value;
        values1[index] /= by;
        values2[index] += values1[index];
    }

    inline float ComputeForward_Sum$(global float$* inputs, int inputsSize, global float$* weights, int idx)
    {
        float$ sum = 0.0f;
        for (int x = 0; x < inputsSize; x++) sum += inputs[x] * Get2$(weights, x, idx, inputsSize);
        return SumComponents$(sum);
    }

    );

    netCode->Using(commonCode);

    // AF

    afCode = make_shared<OCLProgramUnit>(ctx, "af.h");

    ADD_OCL_CODE(afCode,
    inline float Sigmoid(float value, float alpha)
    {
        return (value * alpha) / (1.0f + fabs(value * alpha));
    }
    );

    ADD_OCL_CODE(afCode,
    inline float$ SigmoidD$(float$ value, float alpha)
    {
        float$ a = fabs(value * alpha);
        return alpha * (1.0f / ((1.0f + a) * (1.0f + a)));
    }
    );

    // Reduce

    reduceCode = make_shared<OCLProgramUnit>(ctx, "reduce.h");

    ADD_OCL_CODE(reduceCode,
    
    inline void Reduce_Sum(local float* values)
    {
        int localSize = get_local_size(0);
        int localId = get_local_id(0);

        for (int offset = localSize / 2; offset > 0; offset = offset / 2)
        {
            if (localId < offset)
            {
                values[localId] += values[localId + offset];
            }

            barrier(CLK_LOCAL_MEM_FENCE);
        }
    }

    inline void Reduce_SumFrom(local float* values, int from)
    {
        int localSize = get_local_size(0);
        int localId = get_local_id(0);

        from *= localSize;

        for (int offset = localSize / 2; offset > 0; offset = offset / 2)
        {
            if (localId < offset)
            {
                int idx = from + localId;
                values[idx] += values[idx + offset];
            }

            barrier(CLK_LOCAL_MEM_FENCE);
        }
    }
    );
}
