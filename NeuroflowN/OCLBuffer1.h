#pragma once

#include "OCLTypedefs.h"
#include "OCL.h"
#include <string>
#include "GetSize.h"
#include "IDeviceArray.h"

namespace NeuroflowN
{
    class OCLBuffer1 : public IDeviceArray
    {        
        OCLBuffer1();

        unsigned size;
        cl::Buffer buffer;
    public:
        OCLBuffer1(const cl::Buffer& buffer) :
            buffer(buffer),
            size(buffer.getInfo<CL_MEM_SIZE>() / sizeof(float))
        {
        }

        DeviceArrayType GetType() const
        {
            return DeviceArrayType::DeviceArray;
        }

        unsigned GetSize() const
        {
            return size;
        }

        const cl::Buffer& GetCLBuffer() const
        {
            return buffer;
        }

        void Dump(const OCLIntCtxSPtrT& ctx, std::string title, bool toDebug) const;
    };

    template <>
    struct GetSize<std::reference_wrapper<const OCLBuffer1>>
    {
        inline static unsigned Get(std::reference_wrapper<const OCLBuffer1> buff)
        {
            return buff.get().GetSize();
        }
    };
}