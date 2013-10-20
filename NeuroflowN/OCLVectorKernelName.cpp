#include "stdafx.h"
#include "OCLVectorKernelName.h"
#include "Error.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLVectorKernelName::OCLVectorKernelName(const std::string& name)
{
    unsigned v = 1;
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

const std::string& OCLVectorKernelName::operator()(unsigned vectorSize) const
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