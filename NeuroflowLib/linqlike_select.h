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

    template <typename T, typename F>
    auto operator|(const enumerable<T>& e, const _select<F>& s) -> enumerable<decltype(s.tran()(T()))>
    {
        typedef decltype(s.tran()(T())) result_t;
        return enumerable<result_t>([=]()
        {
            return enumerable<result_t>::pull_type([=](enumerable<result_t>::push_type& yield)
            {
                std::for_each(e.cbegin(), e.cend(), [&](const T& v)
                {
                    yield(s.tran()(v));
                });
            });
        });
    }

    template <typename T, typename F>
    auto operator>>(const enumerable<T>& e, const _select<F>& s) -> decltype(e | s)
    {
        return e | s;
    }
}