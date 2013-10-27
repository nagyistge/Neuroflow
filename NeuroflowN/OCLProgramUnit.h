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

	public:
		OCLProgramUnit(const OCLIntCtxSPtrT& ctx);
		void Using(const std::shared_ptr<OCLProgramUnit>& baseUnit);
		void AddCode(const std::string code);
		std::string GetCode();

		const OCLIntCtxSPtrT& GetIntCtx() const
		{
			return ctx;
		}

	protected:
		OCLIntCtxSPtrT ctx;

		void GetBuilders(std::list<std::stringstream*>& to);

	private:
		std::string CreateNumberedVersions(const std::string& prg);
	};
}