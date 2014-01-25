#pragma once

#include "linqlike_base.h"
#include "linqlike_from.h"
#include "linqlike_select.h"
#include <boost/optional.hpp>

namespace linqlike
{
    template <typename K, typename T>
    struct grouping
    {
        grouping(const K& key, std::vector<T*>&& values) : 
            _key(key), 
            _values(std::move(values)) 
        {
        }

        const K& key() const
        {
            return _key;
        }

        const std::vector<T*>& values() const
        {
            return _values;
        }

    private:
        std::vector<T*> _values;
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

    template <typename F>
    struct _group_by
    {
        explicit _group_by(const F& selectKey) : _selectKey(selectKey) { }

        const F& select_key() const
        {
            return _selectKey;
        }
    private:
        F _selectKey;
    };

    template <typename F>
    _group_by<F> group_by(const F& selectKey)
    {
        return _group_by<F>(selectKey);
    }

    template <typename TColl, typename F, typename T = TColl::value_type>
    auto operator|(TColl& coll, const _group_by<F>& groupdBy)
    {
        typedef decltype(groupdBy.select_key()(_sniff<T>())) key_t;
        typedef grouping<key_t, T> grouping_t;
        TColl* pcoll = &coll;
        return enumerable<grouping_t>([=]() mutable
        {
            return enumerable<grouping_t>::pull_type([=](enumerable<grouping_t>::push_type& yield) mutable
            {
                typedef std::multimap<key_t, T*> map_t;
                typedef typename map_t::value_type map_value_t;
                map_t map;
                auto& selectKey = groupdBy.select_key();
                for (auto& v : *pcoll)
                {
                    map.insert(map_value_t(selectKey(v), &v));
                }

                boost::optional<key_t> current;
                for (auto& v : map)
                {
                    if (v.first != current)
                    {
                        std::vector<T*> items;
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