#include "stdafx.h"
#include "ocl_randomize_weights_uniform.h"

USING

ocl_randomize_weights_uniform::ocl_randomize_weights_uniform(const std::weak_ptr<ocl_computation_context>& context, const learning_behavior_ptr& behavior, const training_node_collection_t& nodes) :
ocl_learning_impl_of(context, behavior, nodes),
learning_impl_of(behavior, nodes)
{
}

void ocl_randomize_weights_uniform::initialize()
{
    throw_not_implemented();
}