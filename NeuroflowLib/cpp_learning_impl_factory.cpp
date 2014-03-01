#include "stdafx.h"
#include "cpp_learning_impl_factory.h"
#include "gradient_descent_learning.h"
#include "randomize_weights_uniform.h"

USING

cpp_learning_impl_factory::factory_map_t cpp_learning_impl_factory::factories = 
{
    { 
        typeid(gradient_descent_learning).name(), 
        [](const learning_behavior_ptr& learningBehavior, const training_node_collection_t& nodes) 
        { 
            return null; 
        } 
    },
    { 
        typeid(randomize_weights_uniform).name(), 
        [](const learning_behavior_ptr& learningBehavior, const training_node_collection_t& nodes) 
        { 
            return null; 
        } 
    }
};

learning_impl_ptr cpp_learning_impl_factory::create_impl(const learning_behavior_ptr& learningBehavior, const training_node_collection_t& nodes)
{
    auto type = typeid(*learningBehavior).name();
    auto result = factories.find(type);
    if (result != factories.end())
    {
        auto impl = result->second(learningBehavior, nodes);
        if (impl) return impl;
    }
    throw_not_implemented(string("Learning for behavior type: '") + type + "' is not implemeted.");
}