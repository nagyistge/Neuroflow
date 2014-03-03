#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename TColl>
    struct _cross_join
    {
        explicit _cross_join(TColl& coll) : _coll(coll) { }

        TColl& coll()
        {
            return _coll;
        }

    private:
        TColl _coll;
    };

    template <typename TColl>
    _cross_join<TColl> cross_join(TColl& other)
    {
        return _cross_join<TColl>(other);
    }

    template <
        typename TColl1, 
        typename TColl2, 
        typename T1 = TColl1::value_type, 
        typename T2 = TColl2::value_type, 
        typename R = std::pair<T1, T2>>
    enumerable<R> operator|(TColl1& coll, _cross_join<TColl2>& s)
    {
        return enumerable<R>([=]() mutable
        {
            return enumerable<R>::make_pull([=](enumerable<R>::push_type& yield) mutable
            {
                for (auto& v1 : coll)
                {
                    for (auto& v2 : s.coll())
                    {
                        yield(std::make_pair(v1, v2));
                    }
                }                
            });
        });
    }
}