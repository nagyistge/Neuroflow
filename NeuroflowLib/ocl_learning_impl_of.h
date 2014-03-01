#pragma once
#include "ocl_nfdev.h"
#include "learning_impl_of.h"
#include "weak_contexted.h"

namespace nf
{
    template <typename T>
    struct ocl_learning_impl_of : weak_contexted<ocl_computation_context>, virtual learning_impl_of<T>
    {
        ocl_learning_impl_of(const std::weak_ptr<ocl_computation_context>& context, const learning_behavior_ptr& behavior, const training_node_collection_t& nodes) :
            weak_contexted(context),
            learning_impl_of(behavior, nodes)
        {
        }
    };
}