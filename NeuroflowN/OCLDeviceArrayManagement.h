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
        friend class OCLDeviceArrayPool;

        OCLIntCtxSPtrT ctx;
        OCLVectorUtilsSPtrT vectorUtils;

        cl::Buffer CreateBuffer(bool copyOptimized, int size);

    public:
        OCLDeviceArrayManagement(const OCLIntCtxSPtrT& ctx, const OCLVectorUtilsSPtrT& vectorUtils) :
            ctx(ctx),
            vectorUtils(vectorUtils)
        {
        }

        IDeviceArray* CreateArray(bool copyOptimized, int size);
        IDeviceArray2* CreateArray2(bool copyOptimized, int rowSize, int colSize);
        void Copy(IDeviceArray* from, int fromIndex, IDeviceArray* to, int toIndex, int size);
        IDeviceArrayPool* CreatePool();
    };
}