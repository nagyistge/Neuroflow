#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename F = _dummy>
    struct _minmax
    {
        _minmax(bool isMin) : _isMin(isMin) { }
        _minmax(bool isMin, const F& sel) : _isMin(isMin), _sel(sel) { }

        const boost::optional<F>& sel() const
        {
            return _sel;
        }

        bool is_min() const
        {
            return _isMin;
        }

    private:
        bool _isMin;
        boost::optional<F> _sel;
    };

    inline _minmax<> min_value()
    {
        return _minmax<>(true);
    }

    template <typename F>
    _minmax<F> min_value(const F& sel)
    {
        return _minmax<F>(true, sel);
    }

    inline _minmax<> max_value()
    {
        return _minmax<>(false);
    }

    template <typename F>
    _minmax<F> max_value(const F& sel)
    {
        return _minmax<F>(false, sel);
    }

    template <typename TColl, typename T = TColl::value_type>
    T operator|(TColl& coll, const _minmax<_dummy>& a)
    {
        boost::optional<T> result;
        for (auto& v : coll)
        {
            if (!result)
            {
                result = v;
            }
            else
            {
                if (a.is_min())
                {
                    if (v < *result) result = v;
                }
                else
                {
                    if (v > *result) result = v;
                }
            }
        }
        if (!result) _throw_seq_empty();
        return *result;
    }

    template <typename TColl, typename F, typename T = TColl::value_type>
    auto operator|(TColl& coll, const _minmax<F>& a) -> decltype((*a.sel())(_wat<T>()))
    {
        typedef decltype((*a.sel())(_wat<T>())) result_t;
        auto& sel = *a.sel();
        boost::optional<result_t> result;
        for (auto& v : coll)
        {
            if (!result)
            {
                result = sel(v);
            }
            else
            {
                auto sv = sel(v);
                if (a.is_min())
                {
                    if (sv < *result) result = sv;
                }
                else
                {
                    if (sv > *result) result = sv;
                }
            }
        }
        if (!result) _throw_seq_empty();
        return *result;
    }
}