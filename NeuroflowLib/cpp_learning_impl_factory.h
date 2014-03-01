#pragma once

#include "cpp_nfdev.h"
#include "learning_impl_factory.h"

namespace nf
{
    struct cpp_learning_impl_factory : virtual learning_impl_factory
    {
    protected:
        factory_map_t get_factories() override;
    };
}