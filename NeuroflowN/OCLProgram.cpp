#include "stdafx.h"
#include "OCLProgram.h"
#include "OCL.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLProgram::OCLProgram(const OCLIntCtxSPtrT& ctx, const std::string& name) :
	OCLProgramUnit(ctx, name)
{
}

cl::Kernel OCLProgram::CreateKernel(const std::string name)
{
    return Kernel(GetProgram(), name.c_str());
}