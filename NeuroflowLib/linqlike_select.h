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
    auto operator|(TColl& coll, const _select<F>& s) 
        //-> enumerable<decltype(s.tran()(_sniff<T>()))> // TODO: Remove this VS hack if there will be better C++14 support!
    {
        typedef decltype(s.tran()(_sniff<T>())) result_t;
        TColl* pcoll = &coll;
        return enumerable<result_t>([=]() mutable
        {
            return enumerable<result_t>::pull_type([=](enumerable<result_t>::push_type& yield) mutable
            {
                for (auto& v : *pcoll)
                {
                    yield(s.tran()(v));
                }
            });
        });
    }
}