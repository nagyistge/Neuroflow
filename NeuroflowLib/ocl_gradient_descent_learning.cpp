#include "stdafx.h"
#include "ocl_gradient_descent_learning.h"

USING

ocl_gradient_descent_learning::ocl_gradient_descent_learning(const std::weak_ptr<ocl_computation_context>& context, const learning_behavior_ptr& behavior, const training_node_collection_t& nodes) :
learning_impl_of(context, behavior, nodes)
{
}

supervised_learning_iteration_type ocl_gradient_descent_learning::iteration_type() const
{
    return behavior()->weight_update_mode() == weight_update_mode::online ? supervised_learning_iteration_type::online : supervised_learning_iteration_type::offline;
}

void ocl_gradient_descent_learning::initialize()
{
}

void ocl_gradient_descent_learning::run(idx_t iterationCount, const device_array_ptr& error)
{
    throw_not_implemented();
}