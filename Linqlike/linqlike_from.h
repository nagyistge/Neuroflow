#pragma once

#include <map>
#include <unordered_map>
#include "linqlike_base.h"

namespace linqlike
{
    template <typename TIterator, typename T = typename TIterator::value_type>
    enumerable<T> from_iterators(TIterator& begin, TIterator& end)
    {
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::make_pull([=](enumerable<T>::push_type& yield) mutable
            {
                std::for_each(begin, end, [&](T& v)
                {
                    yield(v);
                });
            });
        });
    }

    template <typename TIterator, typename T = typename TIterator::value_type>
    enumerable<T> from_iterators(const TIterator& begin, const TIterator& end)
    {
        TIterator& b = const_cast<TIterator&>(begin);
        TIterator& e = const_cast<TIterator&>(end);
        return enumerable<T>([=]() mutable
        {
            return enumerable<T>::make_pull([=](enumerable<T>::push_type& yield) mutable
            {
                std::for_each(b, e, [&](T& v)
                {
                    yield(v);
                });
            });
        });
    }

    template <typename T>
    enumerable<T>& from(const enumerable<T>& e)
    {
        return from(const_cast<enumerable<T>&>(e));
    }

    template <typename T>
    enumerable<T>& from(enumerable<T>& e)
    {
        return e;
    }

    template <typename TColl, typename T = typename TColl::value_type>
    enumerable<T> from(TColl& coll)
    {
        return from_iterators(std::begin(coll), std::end(coll));
    }

    template <typename TColl, typename T = typename TColl::value_type>
    enumerable<T> from(const TColl& coll)
    {
        TColl& c = const_cast<TColl&>(coll);
        return from_iterators(std::begin(c), std::end(c));
    }

    template <typename K, typename V>
    enumerable<std::pair<K, V*>> from(const std::map<K, V>& map)
    {
        return from(const_cast<std::map<K, V>&>(map));
    }

    template <typename K, typename V>
    enumerable<std::pair<K, V*>> from(std::map<K, V>& map)
    {
        auto begin = std::begin(map);
        auto end = std::end(map);
        return enumerable<std::pair<K, V*>>([=]() mutable
        {
            return enumerable<std::pair<K, V*>>::make_pull([=](enumerable<std::pair<K, V*>>::push_type& yield) mutable
            {
                std::for_each(begin, end, [&](std::map<K, V>::iterator::value_type& v)
                {
                    yield(make_pair(K(v.first), &(v.second)));
                });
            });
        });
    }

    template <typename K, typename V>
    enumerable<std::pair<K, V*>> from(const std::unordered_map<K, V>& unordered_map)
    {
        return from(const_cast<std::unordered_map<K, V>&>(unordered_map));
    }

    template <typename K, typename V>
    enumerable<std::pair<K, V*>> from(std::unordered_map<K, V>& unordered_map)
    {
        auto begin = std::begin(unordered_map);
        auto end = std::end(unordered_map);
        return enumerable<std::pair<K, V*>>([=]() mutable
        {
            return enumerable<std::pair<K, V*>>::make_pull([=](enumerable<std::pair<K, V*>>::push_type& yield) mutable
            {
                std::for_each(begin, end, [&](std::unordered_map<K, V>::iterator::value_type& v)
                {
                    yield(make_pair(K(v.first), &(v.second)));
                });
            });
        });
    }

    template <typename K, typename V>
    enumerable<std::pair<K, V*>> from(const std::multimap<K, V>& multimap)
    {
        return from(const_cast<std::multimap<K, V>&>(multimap));
    }

    template <typename K, typename V>
    enumerable<std::pair<K, V*>> from(std::multimap<K, V>& multimap)
    {
        auto begin = std::begin(multimap);
        auto end = std::end(multimap);
        return enumerable<std::pair<K, V*>>([=]() mutable
        {
            return enumerable<std::pair<K, V*>>::make_pull([=](enumerable<std::pair<K, V*>>::push_type& yield) mutable
            {
                std::for_each(begin, end, [&](std::multimap<K, V>::iterator::value_type& v)
                {
                    yield(make_pair(K(v.first), &(v.second)));
                });
            });
        });
    }

    template <typename K, typename V>
    enumerable<std::pair<K, V*>> from(const std::unordered_multimap<K, V>& unordered_multimap)
    {
        return from(const_cast<std::unordered_multimap<K, V>&>(unordered_multimap));
    }

    template <typename K, typename V>
    enumerable<std::pair<K, V*>> from(std::unordered_multimap<K, V>& unordered_multimap)
    {
        auto begin = std::begin(unordered_multimap);
        auto end = std::end(unordered_multimap);
        return enumerable<std::pair<K, V*>>([=]() mutable
        {
            return enumerable<std::pair<K, V*>>::make_pull([=](enumerable<std::pair<K, V*>>::push_type& yield) mutable
            {
                std::for_each(begin, end, [&](std::unordered_multimap<K, V>::iterator::value_type& v)
                {
                    yield(make_pair(K(v.first), &(v.second)));
                });
            });
        });
    }
}