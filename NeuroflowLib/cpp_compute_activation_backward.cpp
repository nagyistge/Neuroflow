#include "stdafx.h"
#include "cpp_compute_activation_backward.h"
#include "cpp_device_array.h"
#include "cpp_device_array2.h"
#include "mlp_backward_node.h"
#include "weighted_errors.h"

USING

void cpp_compute_activation_backward::compute(const nf_object_ptr& context, const std::vector<mlp_backward_node>& nodes, idx_t offset, gradient_computation_formula gcf) const
{
    for (auto& node : nodes)
    {
        if (node.is_last())
        {
            compute_last(node, offset, gcf);
        }
        else if (node.lower_errors.size() != 0)
        {
            compute_inner(node, offset, gcf);
        }
    }
}

void cpp_compute_activation_backward::compute_last(const mlp_backward_node& node, idx_t offset, gradient_computation_formula gcf) const
{
    assert(node.net_outputs);
    auto outputs = dynamic_cast<cpp_device_array*>(node.net_outputs->outputs()().get());
    assert(outputs);
    float* pOutputs = outputs->ptr();
    auto desiredOutputs = dynamic_cast<cpp_device_array*>(node.net_outputs->desired_outputs()().get());
    assert(desiredOutputs);
    float* pDesiredOutputs = desiredOutputs->ptr();
    auto errors = dynamic_cast<cpp_device_array*>(node.errors.get());
    assert(errors);
    float* pErrors = errors->ptr();
    idx_t size = errors->size();
    assert(outputs->size() == size);
    assert(desiredOutputs->size() == size);
    float alpha = node.activation.alpha();

    if (node.activation.function() == activation_function::sigmoid)
    {
        for (idx_t i = 0; i < size; i++)
        {
            pErrors[i] = (pDesiredOutputs[i] - pOutputs[i]) * sigmoid_deriv(pOutputs[i], alpha);
        }
    }
    else
    {
        for (idx_t i = 0; i < size; i++)
        {
            pErrors[i] = (pDesiredOutputs[i] - pOutputs[i]) * alpha;
        }
    }
}

void cpp_compute_activation_backward::compute_inner(const mlp_backward_node& node, idx_t offset, gradient_computation_formula gcf) const
{
    assert(node.lower_errors.size() > 0);
    auto errors = dynamic_cast<cpp_device_array*>(node.errors.get());
    assert(errors);
    float* pErrors = errors->ptr();
    idx_t size = errors->size();
    float alpha = node.activation.alpha();
    
    for (idx_t valueIdx = 0; valueIdx < size; valueIdx++)
    {
        float sum = 0.0f;
        for (auto& weightedError : node.lower_errors)
        {
            auto lowerErrors = dynamic_cast<cpp_device_array*>(weightedError.errors().get());
            assert(lowerErrors);
            float* pLowerErrors = lowerErrors->ptr();
            auto lowerWeights = dynamic_cast<cpp_device_array2*>(weightedError.weights().get());
            assert(lowerWeights);
            float* pLowerWeights = lowerWeights->ptr();
            idx_t lowerErrorsSize = lowerErrors->size();

            for (idx_t lowerErrorIdx = 0; lowerErrorIdx < lowerErrorsSize; lowerErrorIdx++)
            {
                sum += pLowerErrors[lowerErrorIdx] * pLowerWeights[get_index2(valueIdx, lowerErrorIdx, size)];
            }
        }

        if (node.activation.function() == activation_function::sigmoid)
        {
            assert(node.out);
            auto outputs = dynamic_cast<cpp_device_array*>(node.out().get());
            assert(outputs);
            assert(outputs->size() == size);
            float* pOutputs = outputs->ptr();

            pErrors[valueIdx] = sum * sigmoid_deriv(pOutputs[valueIdx], alpha);
        }
        else
        {
            pErrors[valueIdx] = sum * alpha;
        }
    }
}

float cpp_compute_activation_backward::sigmoid_deriv(float value, float alpha)
{
    // return alpha * (1.0f - value * value) / 2.0f; // Logistics
    // return alpha * (1.0f - (value * value)); // Tanh
    return alpha * 1.0f / ((1.0f + abs(value * alpha)) * (1.0f + abs(value * alpha))); // Elliot
}