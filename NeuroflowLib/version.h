#pragma once
#include <string>

#define MAJOR_VER 1
#define MINOR_VER 0
#define BUILD_VER 364
#define REVISION_VER 24305

namespace nf
{
    inline std::wstring version()
    {
        return std::to_wstring(MAJOR_VER) + L"." + std::to_wstring(MINOR_VER) + L"." + std::to_wstring(BUILD_VER) + L"." + std::to_wstring(REVISION_VER);
    }
}