#pragma once
#include "cpp_nfdev.h"
#include "gradient_descent_learning.h"
#include "learning_impl_of.h"
#include "supervised_learning.h"

namespace nf
{
    struct cpp_gradient_descent_learning : virtual learning_impl_of<cpp_computation_context, gradient_descent_learning>, virtual supervised_learning
    {
        cpp_gradient_descent_learning(const std::weak_ptr<cpp_computation_context>& context, const learning_behavior_ptr& behavior, const training_node_collection_ptr& nodes);

        supervised_learning_iteration_type iteration_type() const override;
        void initialize() override;
        void run(idx_t iterationCount, const device_array_ptr& error) override;

    private:
        cpp_device_array_collection_t _deltas;

        void update_weights_online(cpp_device_array* deltas, cpp_device_array* weights, cpp_device_array* gradients);
        void update_weights_offline(cpp_device_array* deltas, cpp_device_array* weights, cpp_device_array* gradientSums, float itCount);
    };
}