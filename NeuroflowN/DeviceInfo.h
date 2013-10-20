#pragma once

#include <string>
#include "Error.h"

namespace NeuroflowN
{
    struct DeviceInfo
    {
        DeviceInfo()
        {
        }

        DeviceInfo(const std::string& id, const std::string& name = "", const std::string& platform = "") :
            ID(id),
            Platform(platform),
            Name(name)
        {
        }            

        std::string Platform, ID, Name;
    };
}
