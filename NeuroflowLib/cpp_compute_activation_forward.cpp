#include "stdafx.h"
#include "cpp_compute_activation_forward.h"
#include "cpp_compute_activation_backward.h"
#include "mlp_forward_node.h"
#include "cpp_device_array.h"
#include "cpp_device_array2.h"

USING

void cpp_compute_activation_forward::compute(const nf_object_ptr& context, const std::vector<mlp_forward_node>& nodes, idx_t offset) const
{
    for (auto& node : nodes)
    {
        idx_t inputLayersCount = node.in.size();
        idx_t layerSize = node.size();

        auto outputs = _fast_cast<cpp_device_array>(node.out());
        assert(outputs);
        float* pOutputs = outputs->ptr() + offset * layerSize;
        auto biases = _fast_cast<cpp_device_array>(node.bias.get());
        assert(biases);
        float* pBiases = biases->ptr();
        
        float* pDerivates = null;
        if (node.derivates)
        {
            auto derivates = _fast_cast<cpp_device_array>(node.derivates.get());
            assert(derivates);
            pDerivates = derivates->ptr();
        }
        
        float alpha = node.activation.alpha();
        for (idx_t valueIdx = 0; valueIdx < layerSize; valueIdx++)
        {
            float sum = pBiases[valueIdx];
            for (auto& weightedInput : node.in)
            {
                auto weights = _fast_cast<cpp_device_array2>(weightedInput.weights().get());
                auto inputs = _fast_cast<cpp_device_array>(weightedInput.inputs()());
                assert(weights);
                assert(inputs);
                idx_t inputsSize = inputs->size();
                float* pInputs = inputs->ptr() + offset * inputsSize;
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
                if (pDerivates != null) pDerivates[valueIdx] = cpp_compute_activation_backward::sigmoid_deriv(sum, alpha);
            }
            else // Linear
            {
                pOutputs[valueIdx] = linear(sum, alpha);
                if (pDerivates != null) pDerivates[valueIdx] = alpha;
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