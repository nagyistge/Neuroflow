#pragma once
#include "ocl_nfdev.h"
#include "randomize_weights_uniform.h"
#include "learning_impl_of.h"

namespace nf
{
    struct ocl_randomize_weights_uniform : learning_impl_of<ocl_computation_context, randomize_weights_uniform>
    {
        ocl_randomize_weights_uniform(const std::weak_ptr<ocl_computation_context>& context, const learning_behavior_ptr& behavior, const training_node_collection_ptr& nodes);

        void initialize() override;
    };
}