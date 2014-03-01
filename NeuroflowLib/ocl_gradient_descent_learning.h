#pragma once
#include "ocl_nfdev.h"
#include "gradient_descent_learning.h"
#include "ocl_learning_impl_of.h"
#include "supervised_learning.h"

namespace nf
{
    struct ocl_gradient_descent_learning : ocl_learning_impl_of<gradient_descent_learning>, virtual supervised_learning
    {
        ocl_gradient_descent_learning(const std::weak_ptr<ocl_computation_context>& context, const learning_behavior_ptr& behavior, const training_node_collection_t& nodes);

        supervised_learning_iteration_type iteration_type() const override;
        void initialize() override;
        void run(idx_t iterationCount, const device_array_ptr& error) override;
    };
}