#pragma once

#include "cpp_nfdev.h"
#include "cpp_device_array.h"
#include "cpp_device_array2.h"
#include "cpp_data_array.h"

namespace nf
{
    inline cpp_device_array* to_cpp(const device_array_ptr& ptr, bool allowNull)
    {
        if (ptr == null)
        {
            if (!allowNull) throw_runtime_error("cl::Device array is null.");
            return null;
        }
        auto result = _fast_cast<cpp_device_array>(ptr.get());
        if (result == null) throw_runtime_error("cl::Device array type is unknonwn.");
        return result;
    }

    inline cpp_device_array2* to_cpp(const device_array2_ptr& ptr, bool allowNull)
    {
        if (ptr == null)
        {
            if (!allowNull) throw_runtime_error("cl::Device array 2 is null.");
            return null;
        }
        auto result = _fast_cast<cpp_device_array2>(ptr.get());
        if (result == null) throw_runtime_error("cl::Device array 2 type is unknonwn.");
        return result;
    }

    inline cpp_device_array* to_cpp(const data_array_ptr& ptr, bool allowNull)
    {
        if (ptr == null)
        {
            if (!allowNull) throw_runtime_error("Data array is null.");
            return null;
        }
        auto result = _fast_cast<cpp_device_array>(ptr.get());
        if (result == null) throw_runtime_error("Data array type is unknonwn.");
        return result;
    }
}