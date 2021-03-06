#pragma once

#include <boost/optional.hpp>

namespace linqlike
{
    template <typename F>
    struct _first_or_default
    {
        _first_or_default() { }
        explicit _first_or_default(const F& pred) : _pred(pred) { }

        const boost::optional<F>& pred() const
        {
            return _pred;
        }
    private:
        boost::optional<F> _pred;
    };

    inline _first_or_default<_dummy> first_or_default()
    {
        return _first_or_default<_dummy>();
    }

    template <typename F>
    _first_or_default<F> first_or_default(const F& pred)
    {
        return _first_or_default<F>(pred);
    }

    template <typename TColl, typename T = TColl::value_type>
    T operator|(TColl& coll, const _first_or_default<_dummy>& f)
    {
        for (auto& v : coll)
        {
            return v;
        }
        return T();
    }

    template <typename TColl, typename F, typename T = TColl::value_type>
    T operator|(TColl& coll, const _first_or_default<F>& f)
    {
        for (auto& v : coll)
        {
            if ((*f.pred())(v))
            {
                return v;
            }
        }
        return T();
    }
}