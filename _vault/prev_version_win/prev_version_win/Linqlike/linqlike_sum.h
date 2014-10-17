#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename F = _dummy>
    struct _sum
    {
        _sum() { }
        _sum(const F& pred) : _pred(pred) { }

        const boost::optional<F>& pred() const
        {
            return _pred;
        }

    private:
        boost::optional<F> _pred;
    };

    inline _sum<> sum()
    {
        return _sum<>();
    }

    template <typename F>
    _sum<F> sum(const F& pred)
    {
        return _sum<F>(pred);
    }

    template <typename TColl, typename T = TColl::value_type>
    T operator|(TColl& coll, const _sum<_dummy>& a)
    {
        T s = 0;
        for (auto& v : coll)
        {
            s += v;
        }
        return s;
    }

    template <typename TColl, typename F, typename T = TColl::value_type>
    T operator|(TColl& coll, const _sum<F>& a)
    {
        auto& pred = *a.pred();
        T s = 0;
        for (auto& v : coll)
        {
            if (pred(v)) s += v;
        }
        return s;
    }
}