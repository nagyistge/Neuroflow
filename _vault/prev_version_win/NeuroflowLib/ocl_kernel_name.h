#pragma once

#include "ocl_nfdev.h"

namespace nf
{
    struct ocl_kernel_name
    {
        ocl_kernel_name(const char* name);
        ocl_kernel_name(const std::string& name) : ocl_kernel_name(name.c_str()) { }

        const std::string& operator()(idx_t vectorSize) const;
        const std::string& name() const;

        static std::string as_name(const std::string& name, idx_t vectorSize)
        {
            return as_name(name.c_str(), vectorSize);
        }

        static std::string as_name(const char* name, idx_t vectorSize);

    private:
        std::vector<std::string> names;
    };
}