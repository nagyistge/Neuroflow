#pragma once

#include "linqlike_base.h"
#include <boost/optional.hpp>
#include <vector>
#include <algorithm>
#include <assert.h>

namespace linqlike
{
    template <typename T>
    struct _sort
    {
        explicit _sort(dir direction) : _direction(direction) { }
        explicit _sort(const T& comparer) : _comparer(comparer) { }

        boost::optional<T>& comparer()
        {
            return _comparer;
        }

        const boost::optional<dir> direction() const
        {
            return _direction;
        }

    private:
        boost::optional<T> _comparer;
        boost::optional<dir> _direction;
    };

    inline _sort<_dummy> sort(dir direction)
    {
        return _sort<_dummy>(direction);
    }

    template <typename F>
    _sort<F> sort(F& comparer)
    {
        return _sort<F>(comparer);
    }

    template <typename TColl, typename TComp, typename T = TColl::value_type>
    enumerable<T> operator|(TColl& coll, _sort<TComp>& orderBy)
    {
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::make_pull([=](enumerable<T>::push_type& yield) mutable
            {
                if (orderBy.comparer())
                {
                    std::vector<T> values;
                    for (auto& v : coll) values.push_back(v);
                    auto comp = [=](T& v1, T& v2) mutable { return (*(orderBy.comparer()))(v1, v2); };
                    std::sort(values.begin(), values.end(), comp);
                    for (auto& v : values)
                    {
                        yield(v);
                    }
                }
            });
        });
    }

    template <typename TColl, typename T = TColl::value_type>
    enumerable<T> operator|(TColl& coll, _sort<_dummy>& orderBy)
    {
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::make_pull([=](enumerable<T>::push_type& yield) mutable
            {
                if (orderBy.direction())
                {
                    std::vector<T> values;
                    for (auto& v : coll) values.push_back(v);

                    if (*orderBy.direction() == dir::asc)
                    {
                        std::sort(values.begin(), values.end(), [](T& v1, T& v2) { return v1 < v2; });
                    }
                    else
                    {
                        std::sort(values.begin(), values.end(), [](T& v1, T& v2) { return v2 < v1; });
                    }

                    for (auto& v : values)
                    {
                        yield(v);
                    }
                }
            });
        });
    }
}