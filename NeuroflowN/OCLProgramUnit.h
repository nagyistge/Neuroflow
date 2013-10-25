#pragma once

#include <list>
#include <string>
#include "OCLTypedefs.h"

namespace NeuroflowN
{
	class OCLProgramUnit
	{
		OCLIntCtxSPtrT ctx;
		std::stringstream programBuilder;
		std::list<std::shared_ptr<OCLProgramUnit>> baseUnits;

	public:
		OCLProgramUnit(const OCLIntCtxSPtrT& ctx);
		void Using(const std::shared_ptr<OCLProgramUnit>& baseUnit);
		void AddCode(const std::string code);

	protected:
		std::string GetCode();

	private:
		std::string CreateNumberedVersions(const std::string& prg);
	};
}