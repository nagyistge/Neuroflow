#pragma once

#include "linqlike_base.h"
#include <list>

namespace linqlike
{
    struct to_list { };

    template <typename TColl, typename T = TColl::value_type>
    std::list<T> operator|(TColl& coll, to_list& v)
    {
        std::list<T> result;
        result.assign(std::begin(coll), std::end(coll));
        return std::move(result);
    }
}