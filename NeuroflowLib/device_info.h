#pragma once

#include "nf.h"

namespace nf
{
    struct device_info
    {
        device_info() = delete;
        device_info(const std::wstring& id, const std::wstring& name = L"", const std::wstring& platform = L"")
        {
            values = std::make_tuple(id, name, platform);
        }

        const std::wstring id() const
        {
            return std::get<0>(values);
        }

        const std::wstring name() const
        {
            return std::get<1>(values);
        }

        const std::wstring platform() const
        {
            return std::get<2>(values);
        }

    private:
        std::tuple<std::wstring, std::wstring, std::wstring> values;
    };
}