#pragma once
#include "OCLTypedefs.h"

namespace NeuroflowN
{
	class OCLVault
	{
		OCLIntCtxSPtrT ctx;
		OCLProgramUnitSPtrT commonCode;
		OCLProgramUnitSPtrT netCode;
		OCLProgramUnitSPtrT afCode;

	public:
		OCLVault(const OCLIntCtxSPtrT& ctx);

		const OCLProgramUnitSPtrT& GetCommonCode() const
		{
			return commonCode;
		}

		const OCLProgramUnitSPtrT& GetNetCode() const
		{
			return netCode;
		}

		const OCLProgramUnitSPtrT& GetAFCode() const
		{
			return afCode;
		}
	};
}