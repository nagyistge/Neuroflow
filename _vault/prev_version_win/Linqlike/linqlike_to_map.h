#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename KS, typename VS = _dummy>
    struct _to_map
    {
        _to_map(const KS& keySelector) : _keySelector(keySelector) { }
        _to_map(const KS& keySelector, const VS& valueSelector) : _keySelector(keySelector), _valueSelector(valueSelector) { }

        const KS& key_selector() const
        {
            return _keySelector;
        }

        const boost::optional<VS>& value_selector() const
        {
            return _valueSelector;
        }

    private:
        KS _keySelector;
        boost::optional<VS> _valueSelector;
    };

    template <typename KS>
    _to_map<KS> to_map(const KS& keySelector)
    {
        return _to_map<KS>(keySelector);
    }

    template <typename KS, typename VS>
    _to_map<KS, VS> to_map(const KS& keySelector, const VS& valueSelector)
    {
        return _to_map<KS, VS>(keySelector, valueSelector);
    }

    template <typename TColl, typename KS, typename T = TColl::value_type>
    auto operator|(TColl& coll, const _to_map<KS, _dummy>& toMap)
    {
        auto& keySelector = toMap.key_selector();
        typedef decltype(keySelector(_wat<T>())) key_t;
        std::map<key_t, T> map;
        for (auto& v : coll)
        {
            map.insert(std::make_pair(keySelector(v), v));
        }
        return std::move(map);
    }

    template <typename TColl, typename KS, typename VS, typename T = TColl::value_type>
    auto operator|(TColl& coll, const _to_map<KS, VS>& toMap)
    {
        auto& keySelector = toMap.key_selector();
        auto& valueSelector = *toMap.value_selector();
        typedef decltype(keySelector(_wat<T>())) key_t;
        typedef decltype(valueSelector(_wat<T>())) value_t;
        std::map<key_t, value_t> map;
        for (auto& v : coll)
        {
            map.insert(std::make_pair(keySelector(v), valueSelector(v)));
        }
        return std::move(map);
    }
}