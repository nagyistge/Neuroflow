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
#if (_MSC_VER && _DEBUG)
        typedef typename TColl::iterator iterator;
        bool found = false;
        iterator result;
        for (auto it = std::begin(coll); it != std::end(coll); it++)
        {
            if (!found)
            {
                result = it;
                found = true;
            }
        }
        if (found) return *result; else return T();
#else
        for (auto& v : coll)
        {
            return v;
        }
        return T();
#endif
    }

    template <typename TColl, typename F, typename T = TColl::value_type>
    T operator|(TColl& coll, const _first_or_default<F>& f)
    {
#if (_MSC_VER && _DEBUG)
        typedef typename TColl::iterator iterator;
        bool found = false;
        iterator result;
        for (auto it = std::begin(coll); it != std::end(coll); it++)
        {
            if (!found && (*f.pred())(*it))
            {
                result = it;
                found = true;
            }
        }
        if (found) return *result; else return T();
#else
        for (auto& v : coll)
        {
            if ((*f.pred())(v))
            {
                return v;
            }
        }
        return T();
#endif
    }
}