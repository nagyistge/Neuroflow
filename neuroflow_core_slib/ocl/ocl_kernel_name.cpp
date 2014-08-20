#include "../stdafx.h"
#include "ocl_kernel_name.h"

USING

ocl_kernel_name::ocl_kernel_name(const char* name)
{
    idx_t v = 1;
    do
    {
        if (v == 1)
        {
            names.emplace_back(name);
        }
        else
        {
            stringstream ss;
            ss << name;
            ss << v;
            names.push_back(ss.str());
        }
        v <<= 1;
    } while (v <= 16);
}

const std::string& ocl_kernel_name::operator()(idx_t vectorSize) const
{
    switch (vectorSize)
    {
        case 1:
            return names[0];
        case 2:
            return names[1];
        case 4:
            return names[2];
        case 8:
            return names[3];
        case 16:
            return names[4];
        default:
            throw_invalid_argument("Ivalid vectorSize argument!");
    }
}

const std::string& ocl_kernel_name::name() const
{
    return names[0];
}

std::string ocl_kernel_name::as_name(const char* name, idx_t vectorSize)
{
    stringstream ss;
    ss << name;
    if (vectorSize > 1) ss << vectorSize;
    return ss.str();
}
