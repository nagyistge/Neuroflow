#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename T>
    struct scast
    {
        typedef T type;
    };

    template <typename T>
    struct dcast
    {
        typedef T type;
    };

    template <typename TColl, typename R, typename T = TColl::value_type>
    auto operator|(TColl& coll, const scast<R>& c)
    {
        TColl* pcoll = &coll;
        return enumerable<R>([=]() mutable
        {
            return enumerable<R>::pull_type([=](enumerable<R>::push_type& yield) mutable
            {
                for (auto& v : *pcoll)
                {
                    yield(static_cast<R>(v));
                }
            });
        });
    }

    template <typename TColl, typename R, typename T = TColl::value_type>
    auto operator|(TColl& coll, const dcast<R>& c)
    {
        typedef std::shared_ptr<R> result_t;
        TColl* pcoll = &coll;
        return enumerable<result_t>([=]() mutable
        {
            return enumerable<result_t>::pull_type([=](enumerable<result_t>::push_type& yield) mutable
            {
                for (auto& v : *pcoll)
                {
                    auto p = std::dynamic_pointer_cast<R>(v);
                    if (p != nullptr) yield(p); else throw std::bad_cast("Invalid cast.");
                }
            });
        });
    }
}