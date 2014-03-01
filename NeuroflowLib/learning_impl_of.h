#pragma once

#include "nfdev.h"
#include "learning_impl.h"
#include "training_node.h"

namespace nf
{
    template <typename T>
    struct learning_impl_of : virtual learning_impl
    {
    protected:
        learning_impl_of(const learning_behavior_ptr& behavior, const training_node_collection_t& nodes) :
            _behavior(std::dynamic_pointer_cast<T>(behavior)),
            _nodes(nodes)
        {
            assert(_behavior);
        }

        const std::shared_ptr<T>& behavior() const
        {
            return _behavior;
        }

        const training_node_collection_t& nodes() const
        {
            return _nodes;
        }

        std::shared_ptr<T> _behavior;
        training_node_collection_t _nodes;
    };
}