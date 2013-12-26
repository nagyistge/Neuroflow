#pragma once

#include <memory>

namespace nf
{
    struct nf_object : std::enable_shared_from_this<nf_object>
    {
        virtual ~nf_object() = default;

    protected:
        template <typename T>
        std::shared_ptr<T> shared_this()
        {
            return std::dynamic_pointer_cast<T>(shared_from_this());
        }
    };
}