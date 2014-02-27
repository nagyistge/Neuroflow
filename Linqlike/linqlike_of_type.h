#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename T>
    struct of_type
    {
        typedef T type;
    };

    template <typename TColl, typename R, typename T = TColl::value_type>
    auto operator|(TColl& coll, const of_type<R>& ot) -> enumerable<std::shared_ptr<R>>
    {
        typedef std::shared_ptr<R> result_t;
        return enumerable<result_t>([=]() mutable
        {
            return enumerable<result_t>::pull_type([=](enumerable<result_t>::push_type& yield) mutable
            {
                for (auto& v : coll)
                {
                    auto p = std::dynamic_pointer_cast<R>(v);
                    if (p != nullptr) yield(p);
                }
            });
        });
    }
}