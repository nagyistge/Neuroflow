#include "stdafx.h"
#include "cpp_learning_impl_factory.h"
#include "cpp_gradient_descent_learning.h"
#include "cpp_randomize_weights_uniform.h"

USING

cpp_learning_impl_factory::factory_map_t cpp_learning_impl_factory::get_factories()
{
    return factory_map_t(
    {
        {
            typeid(gradient_descent_learning).name(),
            [](const learning_behavior_ptr& learningBehavior, const training_node_collection_t& nodes)
            {
                return make_shared<cpp_gradient_descent_learning>(learningBehavior, nodes);
            }
        },
        {
            typeid(randomize_weights_uniform).name(),
            [](const learning_behavior_ptr& learningBehavior, const training_node_collection_t& nodes)
            {
                return make_shared<cpp_randomize_weights_uniform>(learningBehavior, nodes);
            }
        }
    });
}