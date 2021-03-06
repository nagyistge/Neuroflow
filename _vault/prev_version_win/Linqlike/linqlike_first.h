#pragma once

#include <boost/optional.hpp>

namespace linqlike
{
    template <typename F>
    struct _first
    {
        _first() { }
        explicit _first(const F& pred) : _pred(pred) { }

        const boost::optional<F>& pred() const
        {
            return _pred;
        }
    private:
        boost::optional<F> _pred;
    };

    inline _first<_dummy> first()
    {
        return _first<_dummy>();
    }

    template <typename F>
    _first<F> first(const F& pred)
    {
        return _first<F>(pred);
    }

    template <typename TColl, typename T = TColl::value_type>
    T operator|(TColl& coll, const _first<_dummy>& f)
    {
        for (auto& v : coll)
        {
            return v;
        };
        _throw_seq_empty();
    }

    template <typename TColl, typename F, typename T = TColl::value_type>
    T operator|(TColl& coll, const _first<F>& f)
    {
        for (auto& v : coll)
        {
            if ((*f.pred())(v))
            {
                return v;
            }
        }
        _throw_seq_empty();
    }
}