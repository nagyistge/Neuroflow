#pragma once

#include "cpp_nf.h"

inline nf::cpp_device_array_ptr to_cpp(const nf::device_array_ptr& ptr, bool allowNull = true)
{
    if (ptr == null)
    {
        if (!allowNull) throw_runtime_error("Device array is null.");
        return null;
    }
    auto result = std::dynamic_pointer_cast<nf::cpp_device_array>(ptr);
    if (result == null) throw_runtime_error("Device array type is unknonwn.");
    return result;
}

inline nf::cpp_device_array2_ptr to_cpp(const nf::device_array2_ptr& ptr, bool allowNull = true)
{
    if (ptr == null)
    {
        if (!allowNull) throw_runtime_error("Device array 2 is null.");
        return null;
    }
    auto result = std::dynamic_pointer_cast<nf::cpp_device_array2>(ptr);
    if (result == null) throw_runtime_error("Device array 2 type is unknonwn.");
    return result;
}