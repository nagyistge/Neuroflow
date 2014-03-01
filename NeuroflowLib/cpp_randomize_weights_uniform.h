#pragma once
#include "nfdev.h"
#include "randomize_weights_uniform.h"
#include "cpp_learning_impl.h"
#include "initialize_learning.h"

namespace nf
{
    struct cpp_randomize_weights_uniform : virtual cpp_learning_impl<randomize_weights_uniform>, virtual initialize_learning
    {
        cpp_randomize_weights_uniform(const learning_behavior_ptr& behavior, const training_node_collection_t& nodes);

        void initialize() override;
    };
}