#include "stdafx.h"
#include "learning_impl_factory.h"

USING

learning_impl_ptr learning_impl_factory::create_impl(const learning_behavior_ptr& learningBehavior, const training_node_collection_t& nodes)
{
    if (_factories.size() == 0)
    {
        _factories = move(get_factories());
        if (_factories.size() == 0) throw_runtime_error("Learning implemetation factories map is empty.");
    }

    auto type = typeid(*learningBehavior).name();
    auto result = _factories.find(type);
    if (result != _factories.end())
    {
        auto impl = result->second(learningBehavior, nodes);
        if (impl) return impl;
    }
    throw_not_implemented(string("Learning for behavior type: '") + type + "' is not implemeted.");
}