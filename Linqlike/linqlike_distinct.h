#pragma once

#include <boost/optional.hpp>
#include "linqlike_base.h"

namespace linqlike
{
    template <typename T>
    struct _distinct
    {
        _distinct() { }
        explicit _distinct(const T& pred) : _pred(pred) { }

        boost::optional<T>& pred()
        {
            return _pred;
        }

    private:
        boost::optional<T> _pred;
    };

    inline _distinct<_dummy> distinct()
    {
        return _distinct<_dummy>();
    }

    template <typename F>
    _distinct<F> distinct(F& pred)
    {
        return _distinct<F>(pred);
    }

    template <typename TColl, typename TComp, typename T = TColl::value_type>
    enumerable<T> operator|(TColl& coll, _distinct<TComp>& distinct)
    {
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::pull_type([=](enumerable<T>::push_type& yield) mutable
            {
                if (distinct.pred())
                {
                    auto pred = *(distinct.pred());
                    std::vector<T> result;
                    for (auto& v : coll)
                    {
                        auto p = [pred, &v](T& other) { return pred(v, other); };
                        if (std::find_if(result.begin(), result.end(), p) == result.end())
                        result.push_back(v);
                    }
                    for (auto& v : result)
                    {
                        yield(v);
                    }
                }
            });
        });
    }

    template <typename TColl, typename T = TColl::value_type>
    enumerable<T> operator|(TColl& coll, _distinct<_dummy>& distinct)
    {
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::pull_type([=](enumerable<T>::push_type& yield) mutable
            {
                std::vector<T> result;
                for (auto& v : coll)
                {
                    if (std::find(result.begin(), result.end(), v) == result.end())
                        result.push_back(v);
                }
                for (auto& v : result)
                {
                    yield(v);
                }
            });
        });
    }
}