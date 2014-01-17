#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename TColl>
    struct _concat
    {
        typedef typename TColl::value_type value_type;
        typedef enumerable<value_type> enumerable_t;

        explicit _concat(TColl& coll) : _other(from(coll)) { }

        enumerable_t& other()
        {
            return _other;
        }
    private:
        enumerable_t _other;
    };

    template <typename TColl>
    _concat<TColl> concat(TColl& other)
    {
        return _concat<TColl>(other);
    }

    template <typename T, typename TColl>
    enumerable<typename TColl::value_type> operator|(enumerable<T>& e, _concat<TColl>& s)
    {
        typedef typename _concat<TColl>::enumerable_t enumerable_t;
        return enumerable_t([=]() mutable
        {
            return enumerable_t::pull_type([=](enumerable_t::push_type& yield) mutable
            {
                for (auto& v : e)
                {
                    yield(v);
                }
                for (auto& v : s.other())
                {
                    yield(v);
                }
            });
        });
    }

    template <typename T, typename TColl>
    auto operator>>(enumerable<T>& e, _concat<TColl>& s) -> decltype(e | s)
    {
        return e | s;
    }
}