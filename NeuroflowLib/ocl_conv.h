#pragma once

#include "ocl_nfdev.h"
#include "ocl_device_array.h"
#include "ocl_device_array2.h"
#include "ocl_data_array.h"

namespace nf
{
    inline ocl_device_array_ptr to_ocl(const device_array_ptr& ptr, bool allowNull)
    {
        if (ptr == null)
        {
            if (!allowNull) throw_runtime_error("cl::Device array is null.");
            return null;
        }
        auto result = std::dynamic_pointer_cast<ocl_device_array>(ptr);
        if (result == null) throw_runtime_error("cl::Device array type is unknonwn.");
        return result;
    }

    inline ocl_device_array2_ptr to_ocl(const device_array2_ptr& ptr, bool allowNull)
    {
        if (ptr == null)
        {
            if (!allowNull) throw_runtime_error("cl::Device array 2 is null.");
            return null;
        }
        auto result = std::dynamic_pointer_cast<ocl_device_array2>(ptr);
        if (result == null) throw_runtime_error("cl::Device array 2 type is unknonwn.");
        return result;
    }

    inline ocl_data_array_ptr to_ocl(const data_array_ptr& ptr, bool allowNull)
    {
        if (ptr == null)
        {
            if (!allowNull) throw_runtime_error("Data array is null.");
            return null;
        }
        auto result = std::dynamic_pointer_cast<ocl_data_array>(ptr);
        if (result == null) throw_runtime_error("Data array type is unknonwn.");
        return result;
    }
}