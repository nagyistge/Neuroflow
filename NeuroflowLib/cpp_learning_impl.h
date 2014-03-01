#pragma once

#include "cpp_nfdev.h"
#include "learning_impl.h"

namespace nf
{
    template <typename T>
    struct cpp_learning_impl : virtual learning_impl
    {
        friend struct cpp_learning_impl_factory;

    protected:
        const std::shared_ptr<T>& behavior() const
        {
            return _behavior;
        }

        const training_node_collection_t& nodes() const
        {
            return _nodes;
        }

    private:
        cpp_learning_impl(const learning_behavior_ptr& behavior, const training_node_collection_t& nodes) :
            _behavior(std::dynamic_pointer_cast<T>(behavior)),
            _nodes(nodes)
        {
        }

        std::shared_ptr<T> _behavior;
        training_node_collection_t _nodes;
    };
}