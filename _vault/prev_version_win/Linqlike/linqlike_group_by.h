#pragma once

#include "linqlike_base.h"
#include <boost/optional.hpp>

namespace linqlike
{
    template <typename K, typename T>
    struct grouping
    {
        grouping(const K& key, std::vector<T>&& values) : 
            _key(key), 
            _values(std::move(values)) 
        {
        }

        const K& key() const
        {
            return _key;
        }

        const std::vector<T>& values() const
        {
            return _values;
        }

    private:
        std::vector<T> _values;
        K _key;
    };

    template <typename K, typename T>
    inline bool operator==(const grouping<K, T>& g1, const grouping<K, T>& g2)
    {
        return g1.key() == g2.key();
    }

    template <typename K, typename T>
    inline bool operator!=(const grouping<K, T>& g1, const grouping<K, T>& g2)
    {
        return !(g1 == g2);
    }

    template <typename FK, typename FV = _dummy>
    struct _group_by
    {
        explicit _group_by(const FK& selectKey) : _selectKey(selectKey) { }
        explicit _group_by(const FK& selectKey, const FV& selectValue) : _selectKey(selectKey), _selectValue(selectValue) { }

        const FK& select_key() const
        {
            return _selectKey;
        }

        const FV& select_value() const
        {
            return _selectValue;
        }
    private:
        FK _selectKey;
        FV _selectValue;
    };

    template <typename F>
    _group_by<F> group_by(const F& selectKey)
    {
        return _group_by<F>(selectKey);
    }

    template <typename FK, typename FV>
    _group_by<FK, FV> group_by(const FK& selectKey, const FV& selectValue)
    {
        return _group_by<FK, FV>(selectKey, selectValue);
    }

    template <typename TColl, typename F, typename T = TColl::value_type>
    auto operator|(TColl& coll, const _group_by<F, _dummy>& groupdBy)
    {
        typedef decltype(groupdBy.select_key()(_wat<T>())) key_t;
        typedef grouping<key_t, T> grouping_t;
        return enumerable<grouping_t>([=]() mutable
        {
            return enumerable<grouping_t>::make_pull([=](enumerable<grouping_t>::push_type& yield) mutable
            {
                typedef std::unordered_multimap<key_t, T> map_t;
                typedef typename map_t::value_type map_value_t;
                map_t map;
                auto& selectKey = groupdBy.select_key();
                for (auto& v : coll)
                {
                    map.insert(map_value_t(selectKey(v), v));
                }

                boost::optional<key_t> current;
                for (auto& v : map)
                {
                    if (!(v.first == current))
                    {
                        std::vector<T> items;
                        auto range = map.equal_range(v.first);
                        std::for_each(range.first, range.second, [&](map_value_t& v)
                        {
                            items.push_back(v.second);
                        });
                        yield(grouping_t(v.first, std::move(items)));

                        current = v.first;
                    }
                }
            });
        });
    }

    template <typename TColl, typename FK, typename FV, typename T = TColl::value_type>
    auto operator|(TColl& coll, const _group_by<FK, FV>& groupdBy) -> enumerable<grouping<decltype(groupdBy.select_key()(_wat<T>())), decltype(groupdBy.select_value()(_wat<T>()))>>
    {
        typedef decltype(groupdBy.select_key()(_wat<T>())) key_t;
        typedef decltype(groupdBy.select_value()(_wat<T>())) value_t;
        typedef grouping<key_t, value_t> grouping_t;
        return enumerable<grouping_t>([=]() mutable
        {
            return enumerable<grouping_t>::make_pull([=](enumerable<grouping_t>::push_type& yield) mutable
            {
                typedef std::unordered_multimap<key_t, value_t> map_t;
                typedef typename map_t::value_type map_value_t;
                map_t map;
                auto& selectKey = groupdBy.select_key();
                auto& selectValue = groupdBy.select_value();
                for (auto& v : coll)
                {
                    map.insert(map_value_t(selectKey(v), selectValue(v)));
                }

                boost::optional<key_t> current;
                for (auto& v : map)
                {
                    if (!(v.first == current))
                    {
                        std::vector<value_t> items;
                        auto range = map.equal_range(v.first);
                        std::for_each(range.first, range.second, [&](map_value_t& v)
                        {
                            items.push_back(v.second);
                        });
                        yield(grouping_t(v.first, std::move(items)));

                        current = v.first;
                    }
                }
            });
        });
    }
}