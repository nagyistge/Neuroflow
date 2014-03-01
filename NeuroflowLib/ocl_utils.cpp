#include "stdafx.h"
#include "ocl_utils.h"
#include "ocl_computation_context.h"
#include "ocl_units.h"
#include "ocl_conv.h"
#include "ocl_device_array.h"
#include "supervised_batch.h"
#include "ocl_program.h"
#include "ocl_sizes.h"
#include "ocl_data_array.h"

USING

ocl_kernel_name ocl_utils::addMSEName = ocl_kernel_name("AddMSE");
ocl_kernel_name ocl_utils::divName = ocl_kernel_name("Div");
ocl_kernel_name ocl_utils::zeroFName = ocl_kernel_name("ZeroF");

ocl_utils::ocl_utils(const ocl_computation_context_wptr& context) :
    weak_contexted(context),
    generator((std::random_device()() << 16) | std::random_device()()),
    addExec(context, addMSEName),
    divExec(context, divName),
    zeroFExec(context, zeroFName)
{
    memset(z2.s, 0, sizeof(float)* 2);
    memset(z4.s, 0, sizeof(float)* 4);
    memset(z8.s, 0, sizeof(float)* 8);
    memset(z16.s, 0, sizeof(float)* 16);

    build();
}

void ocl_utils::build()
{
    auto ctx = lock_context();
    program = make_shared<ocl_program>(ctx, L"utils.c");

    program->include(ctx->units()->common());

    // Zero
    ADD_OCL_CODE(program,

    kernel void ZeroF$(global float$* buffer, unsigned size)
    {
        int block = size / get_global_size(0) + (size % get_global_size(0) != 0 ? 1 : 0);
        int idx = get_global_id(0) * block;
        int max = idx + block;
        if (max > size) max = size;
        while (idx < max)
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

idx_t ocl_utils::get_preferred_workgroup_size_mul()
{
    auto ctx = lock_context();
    auto k = program->create_kernel(zeroFName(1));
    return (idx_t)k.getWorkGroupInfo<CL_KERNEL_PREFERRED_WORK_GROUP_SIZE_MULTIPLE>(ctx->cl_device());
}

void ocl_utils::randomize_uniform(const device_array_ptr& deviceArray, float min, float max)
{
    auto ctx = lock_context();

    try
    {
        auto da = to_ocl(deviceArray, false);

        uniform_real_distribution<float> uniform_distribution(min, max);
        auto randF = bind(uniform_distribution, ref(generator));
        idx_t size = da->size();
        vector<float> v;
        v.reserve(size);
        for (idx_t i = 0; i < size; i++) v.push_back(randF());

        ctx->cl_queue().enqueueWriteBuffer(
            da->buffer(),
            true, // TODO: To non blocking.
            0,
            size * sizeof(float),
            &v[0]);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void ocl_utils::calculate_mse(supervised_batch& batch, const data_array_ptr& dataArray, idx_t valueIndex)
{
    auto ctx = lock_context();

    try
    {
        auto mseA = to_ocl(dataArray, false);
        idx_t mseASize = mseA->size();

        verify_arg(mseA != nullptr, "mseValues");
        verify_arg(valueIndex >= 0 && valueIndex < mseASize, "valueIndex");

        ctx->cl_queue().enqueueFillBuffer<float>(mseA->buffer(), 0.0f, valueIndex * sizeof(float), sizeof(float));

        float count = 0.0f;

        for (auto& sample : batch.samples())
        {
            for (auto& entry : sample.entries())
            {
                if (entry.has_output())
                {
                    auto desired = to_ocl(entry.desired_output(), false);
                    auto actual = to_ocl(entry.actual_output(), false);
                    add_mse(desired, actual, mseA, valueIndex);
                    count++;
                }
            }
        }

        if (count != 0.0f) div(mseA, valueIndex, count);
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}

void ocl_utils::add_mse(const ocl_data_array_ptr& desiredValues, const ocl_data_array_ptr& currentValues, const ocl_data_array_ptr& mseValues, idx_t mseValueIndex)
{
    idx_t desiredValuesSize = desiredValues->size(), currentValuesSize = currentValues->size(), mseValuesSize = mseValues->size();

    assert(desiredValuesSize == currentValuesSize);
    assert(mseValueIndex < mseValuesSize);

    idx_t vectorSize = get_vector_size(desiredValuesSize);

    addExec.execute(
    program,
    vectorSize,
    [&, vectorSize, mseValueIndex](cl::Kernel& kernel)
    {
    kernel.setArg(0, desiredValues->buffer());
    kernel.setArg(1, currentValues->buffer());
    kernel.setArg(2, (unsigned)(desiredValuesSize / vectorSize));
    kernel.setArg(3, mseValues->buffer());
    kernel.setArg(4, (unsigned)mseValueIndex);
    },
    1);
}

void ocl_utils::div(const ocl_data_array_ptr& values, idx_t valueIndex, float byValue)
{
    assert(valueIndex < values->size());

    divExec.execute(
    program,
    1,
    [=](cl::Kernel& kernel)
    {
    kernel.setArg(0, values->buffer());
    kernel.setArg(1, byValue);
    },
    cl::NDRange(valueIndex),
    cl::NDRange(1),
    cl::NullRange);
}

void ocl_utils::zero(const device_array_ptr& deviceArray)
{
    zero(to_ocl(deviceArray, false)->buffer(), deviceArray->size());
}

void ocl_utils::zero(const cl::Buffer& buffer, idx_t size)
{
    auto ctx = lock_context();

    try
    {
        auto vectorSize = get_vector_size(size);

        if (ctx->is_cpu())
        {
            switch (vectorSize)
            {
                case 2:
                    ctx->cl_queue().enqueueFillBuffer(
                        buffer,
                        z2,
                        0,
                        sizeof(float)* size);
                    break;
                case 4:
                    ctx->cl_queue().enqueueFillBuffer(
                        buffer,
                        z4,
                        0,
                        sizeof(float)* size);
                    break;
                case 8:
                    ctx->cl_queue().enqueueFillBuffer(
                        buffer,
                        z8,
                        0,
                        sizeof(float)* size);
                    break;
                case 16:
                    ctx->cl_queue().enqueueFillBuffer(
                        buffer,
                        z16,
                        0,
                        sizeof(float)* size);
                    break;
                default:
                    ctx->cl_queue().enqueueFillBuffer(
                        buffer,
                        0.0f,
                        0,
                        sizeof(float)* size);
                    break;
            }
        }
        else
        {
            zeroFExec.execute(
            program,
            vectorSize,
            [&](cl::Kernel& kernel)
            {
                kernel.setArg(0, buffer);
                kernel.setArg(1, (unsigned)(size / vectorSize));
            },
            ctx->sizes()->get_optimal_global_size(size, vectorSize));
        }
    }
    catch (exception& ex)
    {
        throw as_ocl_error(ex);
    }
}