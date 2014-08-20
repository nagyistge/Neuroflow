#pragma once

#include "nfdev.h"

namespace nf
{
    template<typename T>
    struct contexted
    {
    protected:
        contexted(const std::shared_ptr<T>& context) : _context(context) { }

        const std::shared_ptr<T>& context() const
        {
            return _context;
        }

    private:
        std::shared_ptr<T> _context;
    };
}