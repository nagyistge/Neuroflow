#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename TIterator, typename T = TIterator::value_type>
    enumerable<T> from_iterators(const TIterator& begin, const TIterator& end)
    {
        return enumerable<T>([=]() { return std::make_shared<iterator_enumerator<TIterator, T>>(begin, end); });
    }

    template <typename TIterator, typename T = const TIterator::value_type>
    enumerable<T> from_const_iterators(const TIterator& begin, const TIterator& end)
    {
        return enumerable<T>([=]() { return std::make_shared<iterator_enumerator<TIterator, T>>(begin, end); });
    }

    template <typename TColl, typename TIterator = TColl::iterator, typename T = TColl::value_type>
    enumerable<T> from(TColl& coll)
    {
        auto begin = coll.begin();
        auto end = coll.end();
        return enumerable<T>([=]() { return std::make_shared<iterator_enumerator<TIterator, T>>(begin, end); });
    }

    template <typename TColl, typename TIterator = TColl::const_iterator, typename T = const TColl::value_type>
    enumerable<T> from_const(TColl& coll)
    {
        auto begin = coll.begin();
        auto end = coll.end();
        return enumerable<T>([=]() { return std::make_shared<iterator_enumerator<TIterator, T>>(begin, end); });
    }
}