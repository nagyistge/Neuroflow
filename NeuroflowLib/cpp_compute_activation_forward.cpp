#include "stdafx.h"
#include "cpp_compute_activation_forward.h"
#include "mlp_forward_node.h"
#include "cpp_device_array.h"
#include "cpp_device_array2.h"

USING

void cpp_compute_activation_forward::compute(const nf_object_ptr& context, const std::vector<mlp_forward_node>& nodes, idx_t offset) const
{
    for (auto& node : nodes)
    {
        auto outputs = dynamic_cast<cpp_device_array*>(node.out());
        assert(outputs);
        float* pOutputs = outputs->ptr();
        idx_t inputLayersCount = node.in.size();
        idx_t layerSize = node.size();
        float alpha = node.activation.alpha();
        for (idx_t valueIdx = 0; valueIdx < layerSize; valueIdx++)
        {
            float sum = 0.0f;
            for (auto& weightedInput : node.in)
            {
                auto weights = dynamic_cast<cpp_device_array2*>(weightedInput.weights().get());
                auto inputs = dynamic_cast<cpp_device_array*>(weightedInput.inputs()());
                assert(weights);
                assert(inputs);
                idx_t inputsSize = inputs->size();
                float* pInputs = inputs->ptr();
                float* pWeights = weights->ptr();
                for (idx_t inputIdx = 0; inputIdx < inputsSize; inputIdx++)
                {
                    idx_t widx = get_index2(inputIdx, valueIdx, inputsSize);
                    assert(widx >= 0 && widx < weights->size());
                    sum += pInputs[inputIdx] * pWeights[widx];
                }
            }

            if (node.activation.function() == activation_function::sigmoid)
            {
                pOutputs[valueIdx] = sigmoid(sum, alpha);
            }
            else // Linear
            {
                pOutputs[valueIdx] = linear(sum, alpha);
            }
        }
    }
}

float cpp_compute_activation_forward::sigmoid(float value, float alpha)
{
    //return (2.0f / (1.0f + exp(-alpha * value))) - 1.0f; // Logistics
    // return (float)Math.Tanh(value * alpha); // Tanh
    return (value * alpha) / (1.0f + abs(value * alpha)); // Elliot
}

float cpp_compute_activation_forward::linear(float value, float alpha)
{
    return nfmin(nfmax(value * alpha, -alpha), alpha);
}