#pragma once

#include <functional>
#include <unordered_map>

// Hashes:
namespace std
{
    template<typename a, typename b>
    struct hash<std::pair<a, b>> 
    {
    private:
       const hash<a> ah;
       const hash<b> bh;
    public:
       hash() : ah(), bh() {}
       size_t operator()(const std::pair<a, b> &p) const 
       {
          return ah(p.first) ^ bh(p.second << 16);
       }
    };
}

namespace NeuroflowN
{
    template <typename TKey, typename TValue>
    class Registry
    {
        std::unordered_map<TKey, TValue> values;

    public:
        TValue GetOrCreate(TKey key, const std::function<TValue()>& factory)
        {
            using namespace std;
            auto it = values.find(key);
            if (it == values.end())
            {
                auto result = factory();
                values.insert(make_pair(key, result));
                return result;
            }
            return it->second;
        }
    };
}