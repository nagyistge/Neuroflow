#pragma once

#include "OCLBuffer1.h"
#include "IDeviceArray2.h"

namespace NeuroflowN
{
    class OCLBuffer2 : public IDeviceArray2
    {
        OCLBuffer2();

        OCLBuffer1 baseBuffer;
        unsigned size1;

    public:
        OCLBuffer2(const cl::Buffer& buffer, unsigned size1) :
            baseBuffer(buffer),
            size1(size1)
        {
        }

        DeviceArrayType GetType() const
        {
            return DeviceArrayType::DeviceArray2;
        }

        unsigned GetSize() const
        {
            return baseBuffer.GetSize();
        }

        unsigned GetSize1() const
        {
            return size1;
        }

        unsigned GetSize2() const
        {
            return GetSize() / GetSize1();
        }

        const cl::Buffer& GetCLBuffer() const
        {
            return baseBuffer.GetCLBuffer();
        }

        const OCLBuffer1& GetBaseBufferCRef() const
        {
            return baseBuffer;
        }

        void Dump(const OCLIntCtxSPtrT& ctx, std::string title, bool toDebug) const
        {
            baseBuffer.Dump(ctx, title, toDebug);
        }
    };

    template <>
    struct GetSize<std::reference_wrapper<const OCLBuffer2>>
    {
        inline static unsigned Get(std::reference_wrapper<const OCLBuffer2> buff)
        {
            return buff.get().GetSize();
        }
    };
}