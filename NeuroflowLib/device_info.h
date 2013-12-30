#pragma once

#include "nf.h"

namespace nf
{
    struct device_info
    {
        device_info() = delete;
        device_info(const std::string& id, const std::string& name = "", const std::string& platform = "")
        {
            values = std::make_tuple(id, name, platform);
        }

        const std::string id() const
        {
            return std::get<0>(values);
        }

        const std::string name() const
        {
            return std::get<1>(values);
        }

        const std::string platform() const
        {
            return std::get<2>(values);
        }

    private:
        std::tuple<std::string, std::string, std::string> values;
    };
}