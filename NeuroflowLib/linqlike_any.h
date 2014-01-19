#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    struct any
    {
    };

    template <typename T>
    bool operator|(enumerable<T>& e, const any& a)
    {
        return e.begin() != e.end();
    }

    template <typename T>
    auto operator>>(enumerable<T>& e, const any& a) -> decltype(e | a)
    {
        return e | a;
    }
}