#pragma once

#include "nfdev.h"

namespace nf
{
    struct device_info
    {
        device_info() = delete;
        device_info(const std::wstring& id, const std::wstring& version, const std::wstring& name = L"", const std::wstring& platform = L"") :
            _id(id),
            _version(version),
            _name(name),
            _platform(platform)
        {
        }

        const std::wstring id() const
        {
            return _id;
        }

        const std::wstring version() const
        {
            return _version;
        }

        const std::wstring name() const
        {
            return _name;
        }

        const std::wstring platform() const
        {
            return _platform;
        }

    private:
        std::wstring _id, _version, _name, _platform;
    };
}

namespace std
{
    inline wstring to_wstring(const nf::device_info& info)
    {
        wstringstream ws;
        ws << info.platform();
        ws << L": ";
        ws << info.name();
        ws << L" ";
        ws << info.version();
        return ws.str();
    }
}
