#pragma once
#include "ocl_nfdev.h"
#include "randomize_weights_uniform.h"
#include "ocl_learning_impl_of.h"

namespace nf
{
    struct ocl_randomize_weights_uniform : ocl_learning_impl_of<randomize_weights_uniform>
    {
        ocl_randomize_weights_uniform(const std::weak_ptr<ocl_computation_context>& context, const learning_behavior_ptr& behavior, const training_node_collection_t& nodes);

        void initialize() override;
    };
}