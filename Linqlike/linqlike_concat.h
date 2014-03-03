#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename TColl>
    struct _concat
    {
        explicit _concat(TColl& coll) : _coll(coll) { }

        TColl& coll()
        {
            return _coll;
        }

    private:
        TColl _coll;
    };

    template <typename TColl>
    _concat<TColl> concat(TColl& other)
    {
        return _concat<TColl>(other);
    }

    template <typename TColl1, typename TColl2, typename T = TColl2::value_type>
    enumerable<T> operator|(TColl1& coll, _concat<TColl2>& s)
    {
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::make_pull([=](enumerable<T>::push_type& yield) mutable
            {
                for (auto& v : coll)
                {
                    yield(v);
                }
                for (auto& v : s.coll())
                {
                    yield(v);
                }
            });
        });
    }
}