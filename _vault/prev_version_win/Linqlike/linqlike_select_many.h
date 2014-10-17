#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename F>
    struct _select_many
    {
        explicit _select_many(const F& tran) : _tran(tran) { }

        const F& tran() const
        {
            return _tran;
        }
    private:
        F _tran;
    };

    template <typename F>
    _select_many<F> select_many(const F& tran)
    {
        return _select_many<F>(tran);
    }

    template <typename TColl, typename F, typename T = TColl::value_type>
    auto operator|(TColl& coll, const _select_many<F>& s)
    {
        typedef typename decltype(s.tran()(_wat<T>()))::value_type result_t;
        return enumerable<result_t>([=]() mutable
        {
            return enumerable<result_t>::make_pull([=](enumerable<result_t>::push_type& yield) mutable
            {
                for (auto& v : coll)
                {
                    auto& coll2 = s.tran()(v);
                    for (auto& v2 : coll2)
                    {
                        yield(v2);
                    }
                }
            });
        });
    }
}