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
            _deltas[idx++] = dynamic_pointer_cast<cpp_device_array>(ctx->cpp_device_array_management()->create_array(false, node.weights()->size()));
        }
    }
    else
    {
        for (auto& d : _deltas) ctx->cpp_utils()->zero(d);
    }
}

void cpp_gradient_descent_learning::run(idx_t iterationCount, const device_array_ptr& error)
{
    throw_not_implemented();
}