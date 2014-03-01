#pragma once
#include "cpp_nfdev.h"
#include "randomize_weights_uniform.h"
#include "learning_impl_of.h"
#include "initialize_learning.h"

namespace nf
{
    struct cpp_randomize_weights_uniform : virtual learning_impl_of<cpp_computation_context, randomize_weights_uniform>, virtual initialize_learning
    {
        cpp_randomize_weights_uniform(const std::weak_ptr<cpp_computation_context>& context, const learning_behavior_ptr& behavior, const training_node_collection_t& nodes);

        void initialize() override;
    };
}