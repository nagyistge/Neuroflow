#pragma once
#include "OCLProgramUnit.h"
#include "OCL.h"

namespace NeuroflowN
{
    class OCLProgram :
        public OCLProgramUnit
    {
        cl::Program program;
#if _DEBUG
        std::string source;
#endif
    public:
        OCLProgram(const OCLProgram&) = delete;
        OCLProgram(const OCLIntCtxSPtrT& ctx, const std::string& name);
        cl::Kernel CreateKernel(const std::string name);

    private:
        cl::Program GetProgram();
        cl::Program Compile();
        cl::Program CreateProgramAndBinary(std::vector<char>& bin);
        cl::Program CreateProgram(const std::vector<char>& bin);
        void Build(cl::Program& program);
    };
}