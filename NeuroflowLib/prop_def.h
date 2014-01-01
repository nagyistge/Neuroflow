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
        T def(std::string propId, T defaultValue, std::function<bool(T)> validator)
        {
            if (overrides)
            {
                auto v = overrides->get_optional<T>(propId);
                if (v) propsToExtend.put(propId, *v);
                if (!validator(*v)) throw_invalid_argument(std::string(std::string("Invalid property value for '") + propId + "'."));
                return *v;
            }
            propsToExtend.put(propId, defaultValue);
            return defaultValue;
        }

    private:
        boost::property_tree::ptree& propsToExtend;
        const boost::optional<boost::property_tree::ptree>& overrides;
    };
}