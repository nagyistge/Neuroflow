#include "stdafx.h"
#include "cpp_randomize_weights_uniform.h"

USING

cpp_randomize_weights_uniform::cpp_randomize_weights_uniform(const learning_behavior_ptr& behavior, const training_node_collection_t& nodes) :
learning_impl_of(behavior, nodes)
{
}

void cpp_randomize_weights_uniform::initialize()
{
    throw_not_implemented();
}