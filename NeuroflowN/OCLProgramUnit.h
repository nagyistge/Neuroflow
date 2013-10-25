#pragma once

#include <list>
#include <string>
#include "OCLTypedefs.h"

#define ADD_OCL_CODE(p,s) p->AddCode(#s);

namespace NeuroflowN
{
	class OCLProgramUnit
	{
		std::stringstream codeBuilder;
		std::list<std::shared_ptr<OCLProgramUnit>> baseUnits;

	public:
		OCLProgramUnit(const OCLIntCtxSPtrT& ctx);
		void Using(const std::shared_ptr<OCLProgramUnit>& baseUnit);
		void AddCode(const std::string code);
		std::string GetCode();

	protected:
		OCLIntCtxSPtrT ctx;

		void GetBuilders(std::list<std::stringstream*>& to);

	private:
		std::string CreateNumberedVersions(const std::string& prg);
	};
}