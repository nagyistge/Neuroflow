#pragma once
#include <string>

#define MAJOR_VER 1
#define MINOR_VER 0
#define BUILD_VER 363
#define REVISION_VER 36606

namespace nf
{
    inline std::string version()
    {
        return std::to_string(MAJOR_VER) + "." + std::to_string(MINOR_VER) + "." + std::to_string(BUILD_VER) + "." + std::to_string(REVISION_VER);
    }
}