#pragma once
#include "OCLTypedefs.h"
#include "IDeviceArrayPool.h"

namespace NeuroflowN
{
    class OCLDeviceArrayPool :
        public IDeviceArrayPool
    {
        friend class OCLBuffer1;

        OCLDeviceArrayManagement* daMan;
        OCLVectorUtilsSPtrT vectorUtils;
        cl::Buffer buffer;
        int endIndex = 0;

        int Reserve(int size);
        cl::Buffer CreateSubBuffer(unsigned beginOffset, unsigned size);

    public:
        OCLDeviceArrayPool(OCLDeviceArrayManagement* daMan, const OCLVectorUtilsSPtrT& vectorUtils);

        bool GetIsAllocated() const;
        IDeviceArray* CreateArray(int size);
        IDeviceArray2* CreateArray2(int rowSize, int colSize);
        void Allocate();
        void Zero();
    };
}
