#pragma once

#include "error.h"
#include "ocl.h"

inline std::runtime_error as_ocl_error_helper(const std::exception& ex, const char* file, const int line)
{
    std::stringstream str;
    str << ex.what();

    auto pce = dynamic_cast<const cl::Error*>(&ex);
    if (pce != nullptr)
    {
        str << " CODE: ";
        str << pce->err();
    }

    str << " (type: ";
    str << typeid(ex).name();
    str << ")";
    str << "\n at ";
    str << file;
    str << ": ";
    str << line;
    return std::runtime_error(str.str().c_str());
}

#define as_ocl_error(ex) as_ocl_error_helper(ex, __FILE__, __LINE__)

#define ocl_error(status, message) as_ocl_error_helper(cl::Error(status, message), __FILE__, __LINE__)
