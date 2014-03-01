#include "stdafx.h"
#include "cpp_gradient_descent_learning.h"

USING

cpp_gradient_descent_learning::cpp_gradient_descent_learning(const learning_behavior_ptr& behavior, const training_node_collection_t& nodes) :
learning_impl_of(behavior, nodes)
{
}

supervised_learning_iteration_type cpp_gradient_descent_learning::iteration_type() const
{
    return behavior()->weight_update_mode() == weight_update_mode::online ? supervised_learning_iteration_type::online : supervised_learning_iteration_type::offline;
}

void cpp_gradient_descent_learning::initialize()
{
}

void cpp_gradient_descent_learning::run(idx_t iterationCount, const device_array_ptr& error)
{
    throw_not_implemented();
}