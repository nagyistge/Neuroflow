#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    struct any
    {
    };

    template <typename TColl>
    bool operator|(TColl& coll, const any& a)
    {
        bool result;
        {
            result = std::begin(coll) != std::end(coll);
        }
        return result;
    }
}