#pragma once
#include "OCLTypedefs.h"

namespace NeuroflowN
{
	class OCLVault
	{
		OCLIntCtxSPtrT ctx;
		OCLProgramUnitSPtrT commonCode;

	public:
		OCLVault(const OCLIntCtxSPtrT& ctx);

		const OCLProgramUnitSPtrT& GetCommonCode() const
		{
			return commonCode;
		}
	};
}