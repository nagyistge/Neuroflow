#pragma once

#include "nfdev.h"

namespace nf
{
    struct learning_impl_factory : virtual nf_object
    {
        typedef std::unordered_map<std::string, std::function<learning_impl_ptr(const learning_behavior_ptr&, const training_node_collection_t&)>> factory_map_t;

        learning_impl_ptr create_impl(const learning_behavior_ptr& learningBehavior, const training_node_collection_t& nodes);

    protected:
        virtual factory_map_t get_factories() = 0;

    private:
        factory_map_t _factories;
    };
}