#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename TIterator, typename T = typename TIterator::value_type>
    enumerable<T> from_iterators(TIterator& begin, TIterator& end)
    {
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::pull_type([=](enumerable<T>::push_type& yield) mutable
            {
                std::for_each(begin, end, [&](T& v)
                {
                    yield(v);
                });
            });
        });
    }

    template <typename TColl, typename TIterator = TColl::iterator, typename T = TColl::value_type>
    enumerable<T> from(TColl& coll)
    {
        return from_iterators(std::begin(coll), std::end(coll));
    }
}