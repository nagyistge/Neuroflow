#pragma once

#include "linqlike_base.h"
#include <functional>

namespace linqlike
{
    template <typename F>
    struct _where
    {
        explicit _where(const F& pred) : _pred(pred) { }

        const F& pred() const
        {
            return _pred;
        }
    private:
        F _pred;
    };

    template <typename F>
    _where<F> where(const F& pred)
    {
        return _where<F>(pred);
    }

    template <typename TColl, typename F, typename T = TColl::value_type>
    enumerable<T> operator|(TColl& coll, const _where<F>& w)
    {
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::make_pull([=](enumerable<T>::push_type& yield) mutable
            {
                for (auto& v : coll)
                {
                    if (w.pred()(v)) yield(v);
                };
            });
        });
    }
}