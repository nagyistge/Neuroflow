#pragma once

#include "DataArray.h"
#include "DataArrayFactory.h"
#include "OCLTypedefs.h"
#include "OCLIntCtx.h"
#include "OCLDataArray.h"

namespace NeuroflowN
{
	class OCLDataArrayFactory : public DataArrayFactory
	{
        OCLIntCtxSPtrT ctx;

    public:
        OCLDataArrayFactory(const OCLIntCtxSPtrT& ctx) :
			ctx(ctx)
		{
		}

        DataArray* Create(unsigned size, float fill)
		{
			return new OCLDataArray(ctx, size, fill);
		}

        DataArray* Create(float* values, unsigned beginPos, unsigned length)
		{
			return new OCLDataArray(ctx, length, values + beginPos, false);
		}

        DataArray* CreateConst(float* values, unsigned beginPos, unsigned length)
		{
			return new OCLDataArray(ctx, length, values + beginPos, true);
		}
	};
}