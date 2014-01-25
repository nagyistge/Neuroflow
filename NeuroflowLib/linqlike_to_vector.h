#pragma once

#include "linqlike_base.h"
#include <vector>

namespace linqlike
{
    struct to_vector { };

    template <typename TColl, typename T = TColl::value_type>
    std::vector<T> operator|(TColl& coll, to_vector& v)
    {
        std::vector<T> result;
        result.assign(std::begin(coll), std::end(coll));
    }
}