#include "stdafx.h"
#include "cpp_compute_activation_forward.h"
#include "mlp_forward_node.h"
#include "cpp_device_array.h"
#include "cpp_device_array2.h"

USING

void cpp_compute_activation_forward::compute(const nf_object_ptr& context, const std::vector<mlp_forward_node>& nodes, idx_t offset)
{
    for (auto& node : nodes)
    {
        auto outputs = dynamic_cast<cpp_device_array*>(node.out().get());
        float* pOutputs = outputs->ptr();
        idx_t inputLayersCount = node.in.size();
        idx_t layerSize = node.size();
        for (idx_t valueIdx = 0; valueIdx < layerSize; valueIdx++)
        {
            float sum = 0.0f;
            for (idx_t inputLayerIdx = 0; inputLayerIdx < inputLayersCount; inputLayerIdx++)
            {
                auto weights = dynamic_cast<cpp_device_array2*>(node.in[inputLayerIdx].weights().get());
                auto inputs = dynamic_cast<cpp_device_array*>(node.in[inputLayerIdx].inputs()().get());
                idx_t inputsSize = inputs->size();
                float* pInputs = inputs->ptr();
                float* pWeights = weights->ptr();
                for (idx_t inputIdx = 0; inputIdx < inputsSize; inputIdx++)
                {
                    sum += pInputs[inputIdx] * pWeights[inputsSize * valueIdx + inputIdx];
                }
            }

            if (node.activation.function() == activation_function::sigmoid)
            {
                pOutputs[valueIdx] = sigmoid(sum, node.activation.alpha());
            }
            else // Linear
            {
                pOutputs[valueIdx] = linear(sum, node.activation.alpha());
            }
        }
    }
}

float cpp_compute_activation_forward::sigmoid(float value, float alpha)
{
    // return (2.0f / (1.0f + (float)Math.Exp(-alpha * value))) - 1.0f; // Logistics
    // return (float)Math.Tanh(value * alpha); // Tanh
    return (value * alpha) / (1.0f + abs(value * alpha)); // Elliot
}

float cpp_compute_activation_forward::linear(float value, float alpha)
{
    return nfmin(nfmax(value * alpha, -alpha), alpha);
}