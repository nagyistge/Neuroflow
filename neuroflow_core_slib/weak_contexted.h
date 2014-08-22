#pragma once

#include "nfdev.h"

namespace nf
{
    template<typename T>
    struct weak_contexted
    {
        weak_contexted() = delete;
        weak_contexted(const weak_contexted<T>&) = delete;
        weak_contexted(weak_contexted<T>&&) = delete;

    protected:
        weak_contexted(const std::weak_ptr<T>& context) : _context(context) { }

        std::shared_ptr<T> lock_context() const
        {
            auto ptr = _context.lock();
            if (!ptr) throw_runtime_error("Context is expired.");
            return ptr;
        }

    private:
        std::weak_ptr<T> _context;
    };
}
