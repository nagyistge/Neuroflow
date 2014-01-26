#pragma once

#include "nfdev.h"

namespace nf
{
    struct prop_def
    {
        prop_def(boost::property_tree::ptree& propsToExtend, const boost::optional<boost::property_tree::ptree>& overrides) :
        propsToExtend(propsToExtend),
        overrides(overrides)
        {
        }

        template <typename T>
        T def(std::string propId, T defaultValue)
        {
            return def<T>(propId, defaultValue, [](T x){ return true; });
        }

        template <typename T>
        T def(std::string propId, T defaultValue, std::function<bool(T)> validator)
        {
            if (overrides)
            {
                auto v = overrides->get_optional<T>(propId);
                if (v)
                {
                    if (!validator(*v)) throw_invalid_argument(std::string(std::string("Invalid property value for '") + propId + "'."));
                    propsToExtend.put(propId, *v);                    
                    return *v;
                }
            }
            propsToExtend.put(propId, defaultValue);
            return defaultValue;
        }

        template <typename T>
        T defEnum(std::string propId, T defaultValue)
        {
            return defEnum<T>(propId, defaultValue, [](T x){ return true; });
        }

        template <typename T>
        T defEnum(std::string propId, T defaultValue, std::function<bool(T)> validator)
        {
            if (overrides)
            {
                auto v = overrides->get_optional<T>(propId);
                if (v)
                {
                    if (!validator(*v)) throw_invalid_argument(std::string(std::string("Invalid property value for '") + propId + "'."));
                    propsToExtend.put(propId, *v);
                    return *v;
                }
                auto sv = overrides->get_optional<std::string>(propId);
                if (sv)
                {
                    T enumV = string_to_enum<T>(*sv);
                    if (!validator(enumV)) throw_invalid_argument(std::string(std::string("Invalid property value for '") + propId + "'."));
                    propsToExtend.put(propId, *sv);                    
                    return enumV;
                }
            }
            propsToExtend.put(propId, enum_to_string(defaultValue));
            return defaultValue;
        }

    private:
        boost::property_tree::ptree& propsToExtend;
        const boost::optional<boost::property_tree::ptree>& overrides;
    };
}