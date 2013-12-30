#include "stdafx.h"
#include "cpp_utils.h"
#include "cpp_conv.h"
#include "cpp_device_array.h"

using namespace std;
using namespace nf;

cpp_utils::cpp_utils() :
generator((std::random_device()() << 16) | std::random_device()())
{
}

void cpp_utils::zero(device_array_ptr& deviceArray) const
{
    auto& cppArray = to_cpp(deviceArray, false);
    memset(cppArray->ptr(), 0, sizeof(float)* cppArray->size());
}

void cpp_utils::randomize_uniform(device_array_ptr& deviceArray, float min, float max) const
{
    auto& cppArray = to_cpp(deviceArray, false);
    uniform_real_distribution<float> uniform_distribution(min, max);
    auto randF = bind(uniform_distribution, ref(generator));
    idx_t size = cppArray->size();
    float* p = cppArray->ptr();
    for (idx_t i = 0; i < size; i++) p[i] = randF();
}

void cpp_utils::calculate_mse(const supervised_batch& batch, data_array_ptr& dataArray, idx_t valueIndex) const
{
    /*auto& cppArray = to_cpp(dataArray, false);

    msePtr[valueIndex] = 0.0f;
    float count = 0.0f;
    foreach(var sample in batch)
    {
        foreach(var entry in sample)
        {
            if (entry.HasOutput)
            {
                var actualOutputs = entry.ActualOutput.ToManaged();
                var desiredOutputs = entry.DesiredOutput.ToManaged();
                fixed(float* pAO = actualOutputs.InternalArray, pDO = desiredOutputs.InternalArray)
                {
                    var aoPtr = actualOutputs.ToPtr(pAO);
                    var doPtr = desiredOutputs.ToPtr(pDO);
                    float cMse = 0.0f;

                    for (int x = 0; x < actualOutputs.Size; x++)
                    {
                        float error = (doPtr[x] - aoPtr[x]) * 0.5f;
                        cMse += error * error;
                    }
                    msePtr[valueIndex] += cMse / (float)actualOutputs.Size;

                    count++;
                }
            }
        }
    }

    if (count != 0.0f) msePtr[valueIndex] /= count;*/
}