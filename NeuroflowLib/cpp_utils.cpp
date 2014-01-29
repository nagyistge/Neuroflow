#include "stdafx.h"
#include "cpp_utils.h"
#include "cpp_conv.h"
#include "cpp_device_array.h"
#include "supervised_batch.h"

USING

cpp_utils::cpp_utils() :
generator((std::random_device()() << 16) | std::random_device()())
{
}

void cpp_utils::zero(const device_array_ptr& deviceArray)
{
    auto& cppArray = to_cpp(deviceArray, false);
    memset(cppArray->ptr(), 0, sizeof(float)* cppArray->size());
}

void cpp_utils::randomize_uniform(const device_array_ptr& deviceArray, float min, float max)
{
    auto& cppArray = to_cpp(deviceArray, false);
    uniform_real_distribution<float> uniform_distribution(min, max);
    auto randF = bind(uniform_distribution, ref(generator));
    idx_t size = cppArray->size();
    float* p = cppArray->ptr();
    for (idx_t i = 0; i < size; i++) p[i] = randF();
}

void cpp_utils::calculate_mse(supervised_batch& batch, const data_array_ptr& dataArray, idx_t valueIndex)
{
    verify_arg(dataArray != null, "dataArray");
    verify_arg(valueIndex >= 0 && valueIndex < dataArray->size(), "valueIndex");

    auto& cppArray = to_cpp(dataArray, false);
    auto ptr = cppArray->ptr();

    ptr[valueIndex] = 0.0f;
    float count = 0.0f;
    for (auto& sample : batch.samples())
    {
        for (auto& entry : sample.entries())
        {
            if (entry.has_output())
            {
                auto& actualOutput = to_cpp(entry.actual_output(), false);
                auto& desiredOutput = to_cpp(entry.desired_output(), false);
                float cMse = 0.0f;

                idx_t size = actualOutput->size();
                float* doPtr = desiredOutput->ptr();
                float* aoPtr = actualOutput->ptr();
                for (idx_t x = 0; x < size; x++)
                {
                    float error = (doPtr[x] - aoPtr[x]) * 0.5f;
                    cMse += error * error;
                }
                ptr[valueIndex] += cMse / (float)size;

                count++;
            }
        }
    }

    if (count != 0.0f) ptr[valueIndex] /= count;
}