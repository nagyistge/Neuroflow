#pragma once
#include "OCLProgramUnit.h"

namespace NeuroflowN
{
	class OCLProgram :
		public OCLProgramUnit
	{
		std::string name;
		cl::Program program;
	public:
		OCLProgram(const OCLProgram&) = delete;
		OCLProgram(const OCLIntCtxSPtrT& ctx, const std::string& name);
		cl::Program GetProgram();

	private:
		cl::Program Compile();
		cl::Program CreateProgramAndBinary(std::vector<char>& bin);
		cl::Program CreateProgram(const std::vector<char>& bin);
        void Build(cl::Program& program);
	};
}