#pragma once

#include "IDeviceArrayManagement.h"
#include "OCLTypedefs.h"
#include "OCLIntCtx.h"
#include "OCLBuffer1.h"
#include "OCLBuffer2.h"

namespace NeuroflowN
{
    class OCLDeviceArrayManagement : public IDeviceArrayManagement
    {
        OCLIntCtxSPtrT ctx;

    public:
        OCLDeviceArrayManagement(const OCLIntCtxSPtrT& ctx) :
            ctx(ctx)
        {
        }

        IDeviceArray* CreateArray(bool copyOptimized, int size);

        IDeviceArray2* CreateArray2(bool copyOptimized, int rowSize, int colSize);

        void Copy(IDeviceArray* from, int fromIndex, IDeviceArray* to, int toIndex, int size);
    };
}