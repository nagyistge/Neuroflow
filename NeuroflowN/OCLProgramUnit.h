#pragma once

#include <list>
#include <string>
#include "OCLTypedefs.h"

#define ADD_OCL_CODE(u,c) u->AddCode(#c);

namespace NeuroflowN
{
	class OCLProgramUnit
	{
		std::stringstream codeBuilder;
		std::list<OCLProgramUnitSPtrT> baseUnits;
        std::string name;
        cl::Program program;

	public:
        OCLProgramUnit(const OCLProgramUnit&) = delete;
        OCLProgramUnit(const OCLIntCtxSPtrT& ctx, const std::string& name);
		void Using(const std::shared_ptr<OCLProgramUnit>& baseUnit);
		void AddCode(const std::string code);

		const OCLIntCtxSPtrT& GetIntCtx() const
		{
			return ctx;
		}

        const std::string& GetName() const
        {
            return name;
        }

	protected:
		OCLIntCtxSPtrT ctx;

        std::string GetCode();
        cl::Program GetProgram();

	private:
		std::string CreateNumberedVersions(const std::string& prg);
        std::vector<std::string> GetBaseHeaderNames();
        std::vector<cl::Program> GetBasePrograms();
        cl::Program Compile();
        cl::Program CreateProgramAndBinary(std::vector<char>& bin);
        cl::Program CreateProgram(const std::vector<char>& bin);
        void Build(cl::Program& program, bool fromBin);
	};
}