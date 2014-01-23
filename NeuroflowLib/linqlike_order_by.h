#pragma once

#include "linqlike_base.h"
#include <boost/optional.hpp>
#include <vector>
#include <algorithm>
#include <assert.h>

namespace linqlike
{
    enum class dir
    {
        asc, desc
    };

    template <typename T>
    struct _order_by
    {
        explicit _order_by(dir direction) : _direction(direction) { }
        explicit _order_by(const T& comparer) : _comparer(comparer) { }

        const boost::optional<T>& comparer() const
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

    inline _order_by<int> order_by(dir direction)
    {
        return _order_by<int>(direction);
    }

    template <typename F>
    auto order_by(const F& comparer)
    {
        return _order_by<F>(comparer);
    }

    template <typename TColl, typename T = TColl::value_type>
    enumerable<T> operator|(TColl& coll, const _order_by<int>& orderBy)
    {
        TColl* pcoll = &coll;
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::pull_type([=](enumerable<T>::push_type& yield) mutable
            {
                if (orderBy.direction())
                {
                    std::vector<std::reference_wrapper<T>> values;
                    for (auto& v : *pcoll) values.push_back(std::ref(v));

                    if (*orderBy.direction() == dir::asc)
                    {
                        std::sort(values.begin(), values.end());
                    }
                    else
                    {
                        std::sort(values.begin(), values.end(), std::greater<std::reference_wrapper<T>>());
                    }

                    for (auto& v : values)
                    {
                        yield(v);
                    }
                }
            });
        });
    }

    template <typename TColl, typename TComp, typename T = TColl::value_type>
    enumerable<T> operator|(TColl& coll, const _order_by<TComp>& orderBy)
    {
        TColl* pcoll = &coll;
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::pull_type([=](enumerable<T>::push_type& yield) mutable
            {
                if (orderBy.comparer())
                {
                    std::vector<std::reference_wrapper<T>> values;
                    for (auto& v : *pcoll) values.push_back(std::ref(v));
                    auto comp = [=](std::reference_wrapper<T>& v1, std::reference_wrapper<T>& v2) { return (*(orderBy.comparer()))(v1, v2); };
                    std::sort(values.begin(), values.end(), comp);
                    for (auto& v : values)
                    {
                        yield(v);
                    }
                }
            });
        });
    }
}