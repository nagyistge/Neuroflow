#include "../stdafx.h"
#include "cpp_compute_activation_backward.h"
#include "cpp_device_array.h"
#include "cpp_device_array2.h"
#include "../mlp_backward_node.h"
#include "../weighted_errors.h"

USING

void cpp_compute_activation_backward::compute(const nf_object_ptr& context, const std::vector<mlp_backward_node>& nodes, idx_t offset, gradient_computation_formula gcf, idx_t internalIterationCount) const
{
    for (auto& node : nodes)
    {
        if (node.is_last())
        {
            compute_last(node, offset);
        }
        else if (node.lower_errors.size() != 0)
        {
            compute_inner(node, offset);
        }
        compute_gradients(node, offset, gcf, internalIterationCount);
    }
}

void cpp_compute_activation_backward::compute_last(const mlp_backward_node& node, idx_t offset) const
{
    assert(node.net_outputs);
    auto outputs = _fast_cast<cpp_device_array>(node.net_outputs->outputs()());
    assert(outputs);
    float* pOutputs = outputs->ptr();
    auto desiredOutputs = _fast_cast<cpp_device_array>(node.net_outputs->desired_outputs()());
    assert(desiredOutputs);
    float* pDesiredOutputs = desiredOutputs->ptr();
    auto errors = _fast_cast<cpp_device_array>(node.errors.get());
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

void cpp_compute_activation_backward::compute_inner(const mlp_backward_node& node, idx_t offset) const
{
    assert(node.lower_errors.size() > 0);
    auto errors = _fast_cast<cpp_device_array>(node.errors.get());
    assert(errors);
    float* pErrors = errors->ptr();
    idx_t size = errors->size();
    float alpha = node.activation.alpha();
    
    for (idx_t valueIdx = 0; valueIdx < size; valueIdx++)
    {
        float sum = 0.0f;
        for (auto& weightedError : node.lower_errors)
        {
            auto lowerErrors = _fast_cast<cpp_device_array>(weightedError.errors().get());
            assert(lowerErrors);
            float* pLowerErrors = lowerErrors->ptr();
            auto lowerWeights = _fast_cast<cpp_device_array2>(weightedError.weights().get());
            assert(lowerWeights);
            float* pLowerWeights = lowerWeights->ptr();
            idx_t lowerErrorsSize = lowerErrors->size();

            for (idx_t lowerErrorIdx = 0; lowerErrorIdx < lowerErrorsSize; lowerErrorIdx++)
            {
                idx_t lwidx = get_index2(valueIdx, lowerErrorIdx, size);
                assert(lwidx >= 0 && lwidx < lowerWeights->size());
                sum += pLowerErrors[lowerErrorIdx] * pLowerWeights[lwidx];
            }
        }

        if (node.activation.function() == activation_function::sigmoid)
        {
            assert(node.out);
            auto outputs = _fast_cast<cpp_device_array>(node.out());
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

void cpp_compute_activation_backward::compute_gradients(const mlp_backward_node& node, idx_t offset, gradient_computation_formula gcf, idx_t internalIterationCount) const
{
    switch (gcf)
    {
        case gradient_computation_formula::ff:
            compute_gradients_ff(node, offset);
            break;
        case gradient_computation_formula::bptt_phase1:
            compute_gradients_bpttp1(node, offset);
            break;
        case gradient_computation_formula::bptt_phase2:
            compute_gradients_bpttp2(node, offset, internalIterationCount);
            break;
    }
}

void cpp_compute_activation_backward::compute_gradients_ff(const mlp_backward_node& node, idx_t offset) const
{
    bool hasGradients = node.has_gradients();
    bool hasGradientSums = node.has_gradient_sums();
    assert(hasGradients || hasGradientSums);
    auto errors = _fast_cast<cpp_device_array>(node.errors.get());
    assert(errors);
    float* pErrors = errors->ptr();
    idx_t size = errors->size();

    float* pBiasGradients = null;
    float* pBiasGradientSums = null;
    if (hasGradients)
    {
        auto biasGradients = _fast_cast<cpp_device_array>(node.bias_gradients.get());
        assert(biasGradients);
        pBiasGradients = biasGradients->ptr();
    }
    if (hasGradientSums)
    {
        auto biasGradientSums = _fast_cast<cpp_device_array>(node.bias_gradient_sums.get());
        assert(biasGradientSums);
        pBiasGradientSums = biasGradientSums->ptr();
    }

    for (idx_t valueIdx = 0; valueIdx < size; valueIdx++)
    {
        if (hasGradients) pBiasGradients[valueIdx] = pErrors[valueIdx];
        if (hasGradientSums) pBiasGradientSums[valueIdx] += pErrors[valueIdx];

        idx_t inputLayersCount = node.in.size();
        for (idx_t ilidx = 0; ilidx < inputLayersCount; ilidx++)
        {
            auto inputs = _fast_cast<cpp_device_array>(node.in[ilidx]());
            assert(inputs);
            float* pInputs = inputs->ptr();
            idx_t inputSize = inputs->size();
            if (hasGradients && hasGradientSums)
            {
                auto gradients = _fast_cast<cpp_device_array2>(node.gradients[ilidx].get());
                assert(gradients);
                float* pGradients = gradients->ptr();
                auto gradientSums = _fast_cast<cpp_device_array2>(node.gradient_sums[ilidx].get());
                assert(gradientSums);
                float* pGradientSums = gradientSums->ptr();

                for (idx_t inputIndex = 0; inputIndex < inputSize; inputIndex++)
                {
                    idx_t gidx = get_index2(inputIndex, valueIdx, inputSize);
                    assert(gidx >= 0 && gidx < gradients->size() && gidx < gradientSums->size());
                    pGradientSums[gidx] += (pGradients[gidx] = pInputs[inputIndex] * pErrors[valueIdx]);
                }
            }
            else if (hasGradients)
            {
                auto gradients = _fast_cast<cpp_device_array2>(node.gradients[ilidx].get());
                assert(gradients);
                float* pGradients = gradients->ptr();

                for (idx_t inputIndex = 0; inputIndex < inputSize; inputIndex++)
                {
                    idx_t gidx = get_index2(inputIndex, valueIdx, inputSize);
                    assert(gidx >= 0 && gidx < gradients->size());
                    pGradients[gidx] = pInputs[inputIndex] * pErrors[valueIdx];
                }
            }
            else
            {
                assert(hasGradientSums);
                auto gradientSums = _fast_cast<cpp_device_array2>(node.gradient_sums[ilidx].get());
                assert(gradientSums);
                float* pGradientSums = gradientSums->ptr();

                for (idx_t inputIndex = 0; inputIndex < inputSize; inputIndex++)
                {
                    idx_t gidx = get_index2(inputIndex, valueIdx, inputSize);
                    assert(gidx >= 0 && gidx < gidx < gradientSums->size());
                    pGradientSums[gidx] += (pInputs[inputIndex] * pErrors[valueIdx]);
                }
            }
        }
    }
}

void cpp_compute_activation_backward::compute_gradients_bpttp1(const mlp_backward_node& node, idx_t offset) const
{
    auto errors = _fast_cast<cpp_device_array>(node.errors.get());
    assert(errors);
    float* pErrors = errors->ptr();
    idx_t size = errors->size();

    auto biasGradients = _fast_cast<cpp_device_array>(node.bias_gradients.get());
    assert(biasGradients);
    float* pBiasGradients = biasGradients->ptr();

    for (idx_t valueIdx = 0; valueIdx < size; valueIdx++)
    {
        pBiasGradients[valueIdx] += pErrors[valueIdx];

        idx_t inputLayersCount = node.in.size();
        for (idx_t ilidx = 0; ilidx < inputLayersCount; ilidx++)
        {
            auto inputs = _fast_cast<cpp_device_array>(node.in[ilidx]());
            assert(inputs);
            float* pInputs = inputs->ptr();
            idx_t inputSize = inputs->size();
            auto gradients = _fast_cast<cpp_device_array2>(node.gradients[ilidx].get());
            assert(gradients);
            float* pGradients = gradients->ptr();

            for (idx_t inputIndex = 0; inputIndex < inputSize; inputIndex++)
            {
                idx_t gidx = get_index2(inputIndex, valueIdx, inputSize);
                pGradients[gidx] += pInputs[inputIndex] * pErrors[valueIdx];
            }
        }
    }
}

void cpp_compute_activation_backward::compute_gradients_bpttp2(const mlp_backward_node& node, idx_t offset, idx_t internalIterationCount) const
{
    bool hasGradients = node.has_gradients();
    bool hasGradientSums = node.has_gradient_sums();
    assert(hasGradients || hasGradientSums);
    auto errors = _fast_cast<cpp_device_array>(node.errors.get());
    assert(errors);
    float* pErrors = errors->ptr();
    idx_t size = errors->size();
    float by = internalIterationCount;

    auto biasGradients = _fast_cast<cpp_device_array>(node.bias_gradients.get());
    assert(biasGradients);
    float* pBiasGradients = biasGradients->ptr();

    float* pBiasGradientSums = null;
    if (hasGradientSums)
    {
        auto biasGradientSums = _fast_cast<cpp_device_array>(node.bias_gradient_sums.get());
        assert(biasGradientSums);
        pBiasGradientSums = biasGradientSums->ptr();
    }

    for (idx_t valueIdx = 0; valueIdx < size; valueIdx++)
    {
        pBiasGradients[valueIdx] += pErrors[valueIdx];
        pBiasGradients[valueIdx] /= by;
        if (hasGradientSums) pBiasGradientSums[valueIdx] += pBiasGradients[valueIdx];

        idx_t inputLayersCount = node.in.size();
        for (idx_t ilidx = 0; ilidx < inputLayersCount; ilidx++)
        {
            auto inputs = _fast_cast<cpp_device_array>(node.in[ilidx]());
            assert(inputs);
            float* pInputs = inputs->ptr();
            idx_t inputSize = inputs->size();
            if (hasGradientSums)
            {
                auto gradients = _fast_cast<cpp_device_array2>(node.gradients[ilidx].get());
                assert(gradients);
                float* pGradients = gradients->ptr();
                auto gradientSums = _fast_cast<cpp_device_array2>(node.gradient_sums[ilidx].get());
                assert(gradientSums);
                float* pGradientSums = gradientSums->ptr();

                for (idx_t inputIndex = 0; inputIndex < inputSize; inputIndex++)
                {
                    idx_t gidx = get_index2(inputIndex, valueIdx, inputSize);
                    pGradients[gidx] += pInputs[inputIndex] * pErrors[valueIdx];
                    pGradients[gidx] /= by;
                    pGradientSums[gidx] += pGradients[gidx];
                }
            }
            else
            {
                auto gradients = _fast_cast<cpp_device_array2>(node.gradients[ilidx].get());
                assert(gradients);
                float* pGradients = gradients->ptr();

                for (idx_t inputIndex = 0; inputIndex < inputSize; inputIndex++)
                {
                    idx_t gidx = get_index2(inputIndex, valueIdx, inputSize);
                    pGradients[gidx] += pInputs[inputIndex] * pErrors[valueIdx];
                    pGradients[gidx] /= by;
                }
            }
        }
    }
}
