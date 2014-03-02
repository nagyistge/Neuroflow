#include "stdafx.h"
#include "ocl_learning_impl_factory.h"
#include "ocl_gradient_descent_learning.h"
#include "ocl_randomize_weights_uniform.h"

USING

ocl_learning_impl_factory::factory_map_t ocl_learning_impl_factory::get_factories()
{
    return factory_map_t(
    {
        {
            typeid(gradient_descent_learning).name(),
            [&](const learning_behavior_ptr& learningBehavior, const training_node_collection_ptr& nodes)
            {
                auto ctx = lock_context();
                return make_shared<ocl_gradient_descent_learning>(ctx, learningBehavior, nodes);
            }
        },
        {
            typeid(randomize_weights_uniform).name(),
            [&](const learning_behavior_ptr& learningBehavior, const training_node_collection_ptr& nodes)
            {
                auto ctx = lock_context();
                return make_shared<ocl_randomize_weights_uniform>(ctx, learningBehavior, nodes);
            }
        }
    });
}