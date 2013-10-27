#pragma once
#include "OCLProgramUnit.h"

namespace NeuroflowN
{
	class OCLProgram :
		public OCLProgramUnit
	{
	public:
		OCLProgram(const OCLProgram&) = delete;
		OCLProgram(const OCLIntCtxSPtrT& ctx, const std::string& name);

        cl::Kernel CreateKernel(const std::string name);
	};
}