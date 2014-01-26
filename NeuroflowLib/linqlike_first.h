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

    inline void _throw_seq_empty()
    {
        throw std::runtime_error("Sequence contains no elements.");
    }

    template <typename TColl, typename T = TColl::value_type>
    T operator|(TColl& coll, const _first<_dummy>& f)
    {
        T* result;
        {
            for (auto& v : coll)
            {
                result = &v;
                goto ok;
            };
            _throw_seq_empty();
        }

    ok:
        return *result;
    }

    template <typename TColl, typename F, typename T = TColl::value_type>
    T operator|(TColl& coll, const _first<F>& f)
    {
        T* result;
        {
            for (auto& v : coll)
            {
                if ((*f.pred())(v))
                {
                    result = &v;
                    goto ok;
                }
            }
        }
        _throw_seq_empty();

    ok:
        return *result;
    }
}