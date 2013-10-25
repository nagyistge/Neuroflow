#include "stdafx.h"
#include "OCLProgram.h"
#include "OCLIntCtx.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLProgram::OCLProgram(const OCLIntCtxSPtrT& ctx, const std::string& name) :
	OCLProgramUnit(ctx),
	name(name)
{
}

cl::Program OCLProgram::GetProgram()
{
	if (program() == null) Compile();
	assert(program() != null);
	return program;
}

void OCLProgram::Compile()
{
	assert(program() == null);

	auto toAlphanumeric = [](const string& str)
	{
		stringstream result;
		for (auto ch : str)
		{
			if ((ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')) result << ch; else result << '_';
		}
		return result.str();
	};

	stringstream fns;
	fns << toAlphanumeric(name);
	fns << '_';
	fns << toAlphanumeric(ctx->GetDeviceInfo().ID);
	fns << ".bin";
	string fn = fns.str();
	

}