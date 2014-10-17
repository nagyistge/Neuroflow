#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename F = _dummy>
    struct _size
    {
        _size() { }
        _size(const F& pred) : _pred(pred) { }

        const boost::optional<F>& pred() const
        {
            return _pred;
        }

    private:
        boost::optional<F> _pred;
    };

    inline _size<> size()
    {
        return _size<>();
    }

    template <typename F>
    _size<F> size(const F& pred)
    {
        return _size<F>(pred);
    }

    template <typename TColl>
    ::size_t operator|(TColl& coll, const _size<_dummy>& a)
    {
        ::size_t s = 0;
        auto end = std::end(coll);
        for (auto it = std::begin(coll); it != end; it++)
        {
            s++;
        }
        return s;
    }

    template <typename TColl, typename F>
    ::size_t operator|(TColl& coll, const _size<F>& a)
    {
        auto& pred = *a.pred();
        ::size_t s = 0;
        for (auto& v : coll) if (pred(v)) s++;
        return s;
    }
}