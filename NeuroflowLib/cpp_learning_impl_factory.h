#pragma once

#include "cpp_nfdev.h"
#include "learning_impl_factory.h"

namespace nf
{
    struct cpp_learning_impl_factory : virtual learning_impl_factory
    {
        learning_impl_ptr create_impl(const learning_behavior_ptr& learningBehavior, const training_node_collection_t& nodes) override;

    private:
        typedef std::unordered_map<std::string, std::function<learning_impl_ptr(const learning_behavior_ptr&, const training_node_collection_t&)>> factory_map_t;
        
        static factory_map_t factories;
    };
}