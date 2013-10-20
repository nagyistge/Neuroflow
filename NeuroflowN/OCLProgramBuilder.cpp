#include "stdafx.h"
#include "OCLProgramBuilder.h"
#include "Error.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLProgramBuilder::OCLProgramBuilder(const cl::Context& clContext, const cl::Device& clDevice) :
    clContext(clContext), clDevice(clDevice)
{
    programBuilder << "#pragma OPENCL EXTENSION cl_khr_local_int32_base_atomics : enable\n";
}

void OCLProgramBuilder::Add(const std::string& programString)
{
    if (find(programString.cbegin(), programString.cend(), '$') != programString.cend())
    {
        auto prg = CreateNumberedVersions(programString);
        programBuilder << prg;
    }
    else
    {
        programBuilder << programString;
    }    
}

cl::Program OCLProgramBuilder::Compile()
{
    auto p = Program(clContext, programBuilder.str(), false);

    try
    {
        p.build(vector<Device>(1, clDevice));
    }
    catch (Error&)
    {
        auto info = p.getBuildInfo<CL_PROGRAM_BUILD_LOG>(clDevice);
        info = string("\nOPENCL BUILD FAILED:\n") + info;
        throw_logic_error(info.c_str());
    }

    return p;
}

std::string OCLProgramBuilder::CreateNumberedVersions(const std::string& prg)
{
    stringstream result;
    unsigned v = 1;
    do
    {
        string p = prg, vstr = to_string(v);
        boost::replace_all(p, "$$", vstr);
        if (v == 1)
        {
            boost::replace_all(p, "$", "");
        }
        else
        {
            boost::replace_all(p, "$", vstr);
        }
        result << p;
        v <<= 1;
    }
    while (v <= 16);
    return result.str();
}