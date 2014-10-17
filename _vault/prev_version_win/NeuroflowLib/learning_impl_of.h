#pragma once

#include "nfdev.h"
#include "learning_impl.h"
#include "training_node.h"
#include "weak_contexted.h"

namespace nf
{
    template <typename C, typename T>
    struct learning_impl_of : weak_contexted<C>, virtual learning_impl
    {
    protected:
        learning_impl_of(const std::weak_ptr<C>& context, const learning_behavior_ptr& behavior, const training_node_collection_ptr& nodes) :
            weak_contexted(context),
            _behavior(std::dynamic_pointer_cast<T>(behavior)),
            _nodes(nodes)
        {
            assert(_behavior);
        }

        const std::shared_ptr<T>& behavior() const
        {
            return _behavior;
        }

        const training_node_collection_ptr& nodes() const
        {
            return _nodes;
        }

        std::shared_ptr<T> _behavior;
        training_node_collection_ptr _nodes;
    };
}