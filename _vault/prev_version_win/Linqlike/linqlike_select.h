#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename F>
    struct _select
    {
        explicit _select(const F& tran) : _tran(tran) { }

        const F& tran() const
        {
            return _tran;
        }
    private:
        F _tran;
    };

    template <typename F>
    _select<F> select(const F& tran)
    {
        return _select<F>(tran);
    }

    template <typename TColl, typename F, typename T = TColl::value_type>
    auto operator|(TColl& coll, const _select<F>& s) -> enumerable<decltype(s.tran()(_wat<T>()))>
    {
        typedef decltype(s.tran()(_wat<T>())) result_t;
        return enumerable<result_t>([=]() mutable
        {
            return enumerable<result_t>::make_pull([=](enumerable<result_t>::push_type& yield) mutable
            {
                for (auto& v : coll)
                {
                    yield(s.tran()(v));
                }
            });
        });
    }
}