#include "stdafx.h"
#include "cpp_gradient_descent_learning.h"
#include "training_node.h"
#include "cpp_device_array.h"
#include "cpp_computation_context.h"
#include "cpp_device_array_management.h"
#include "cpp_utils.h"

USING

cpp_gradient_descent_learning::cpp_gradient_descent_learning(const std::weak_ptr<cpp_computation_context>& context, const learning_behavior_ptr& behavior, const training_node_collection_ptr& nodes) :
learning_impl_of(context, behavior, nodes)
{
}

supervised_learning_iteration_type cpp_gradient_descent_learning::iteration_type() const
{
    return behavior()->weight_update_mode() == weight_update_mode::online ? supervised_learning_iteration_type::online : supervised_learning_iteration_type::offline;
}

void cpp_gradient_descent_learning::initialize()
{
    auto ctx = lock_context();
    if (_deltas.size() == 0)
    {
        _deltas.reserve(nodes()->size());
        idx_t idx = 0;
        for (auto& node : *nodes())
        {
            _deltas.emplace_back(move(dynamic_pointer_cast<cpp_device_array>(ctx->cpp_device_array_management()->create_array(false, node.weights()->size()))));
        }
    }
    else
    {
        for (auto& d : _deltas) ctx->cpp_utils()->zero(d);
    }
}

void cpp_gradient_descent_learning::run(idx_t iterationCount, const device_array_ptr& error)
{
    if (behavior()->weight_update_mode() == weight_update_mode::online)
    {
        idx_t idx = 0;
        for (auto& node : *nodes())
        {
            update_weights_online(_deltas[idx++].get(), dynamic_cast<cpp_device_array*>(node.weights().get()), dynamic_cast<cpp_device_array*>(node.gradients().get()));
        }
    }
    else
    {
        idx_t idx = 0;
        float itCount = iterationCount;
        for (auto& node : *nodes())
        {
            update_weights_offline(_deltas[idx++].get(), dynamic_cast<cpp_device_array*>(node.weights().get()), dynamic_cast<cpp_device_array*>(node.gradient_sums().get()), itCount);
        }
    }
}

void cpp_gradient_descent_learning::update_weights_online(cpp_device_array* deltas, cpp_device_array* weights, cpp_device_array* gradients)
{
    assert(weights);
    assert(gradients);
    idx_t size = weights->size();
    assert(gradients->size() == size);
    assert(deltas->size() == size);
    float* weightsPtr = weights->ptr();
    float* gradientsPtr = gradients->ptr();
    float* deltasPtr = deltas->ptr();
    float rate = behavior()->learning_rate();
    float momentum = behavior()->momentum();
    if (behavior()->smoothing())
    {
        float smoothV = 1.0f - momentum;
        for (idx_t idx = 0; idx < size; idx++)
        {
            float update = gradientsPtr[idx] * rate;
            float lastUpdate = gradientsPtr[idx];
            update = (lastUpdate * momentum) + (update * smoothV);
            weightsPtr[idx] += update;
            gradientsPtr[idx] = update;
        }
    }
    else
    {
        for (idx_t idx = 0; idx < size; idx++)
        {
            float update = gradientsPtr[idx] * rate;
            float lastUpdate = deltasPtr[idx];
            update = (lastUpdate * momentum) + update;
            weightsPtr[idx] += update;
            deltasPtr[idx] = update;
        }
    }
}

void cpp_gradient_descent_learning::update_weights_offline(cpp_device_array* deltas, cpp_device_array* weights, cpp_device_array* gradientSums, float itCount)
{
    assert(weights);
    assert(gradientSums);
    idx_t size = weights->size();
    assert(gradientSums->size() == size);
    assert(deltas->size() == size);
    float* weightsPtr = weights->ptr();
    float* gradientSumsPtr = gradientSums->ptr();
    float* deltasPtr = deltas->ptr();
    float rate = behavior()->learning_rate();
    float momentum = behavior()->momentum();
    if (behavior()->smoothing())
    {
        float smoothV = 1.0f - momentum;
        for (idx_t idx = 0; idx < size; idx++)
        {
            float update = (gradientSumsPtr[idx] * rate) / itCount;
            float lastUpdate = gradientSumsPtr[idx];
            update = (lastUpdate * momentum) + (update * smoothV);
            weightsPtr[idx] += update;
            gradientSumsPtr[idx] = update;
        }
    }
    else
    {
        for (idx_t idx = 0; idx < size; idx++)
        {
            float update = (gradientSumsPtr[idx] * rate) / itCount;
            float lastUpdate = gradientSumsPtr[idx];
            update = (lastUpdate * momentum) + update;
            weightsPtr[idx] += update;
            gradientSumsPtr[idx] = update;
        }
    }
}