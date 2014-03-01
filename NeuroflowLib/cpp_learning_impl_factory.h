#pragma once

#include "cpp_nfdev.h"
#include "learning_impl_factory.h"
#include "weak_contexted.h"

namespace nf
{
    struct cpp_learning_impl_factory : weak_contexted<cpp_computation_context>, virtual learning_impl_factory
    {
        cpp_learning_impl_factory(const std::weak_ptr<cpp_computation_context>& context);
    protected:
        factory_map_t get_factories() override;
    };
}