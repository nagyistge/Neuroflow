#pragma once

#include "DataArray.h"
#include "OCLTypedefs.h"
#include "OCLBuffer1.h"

namespace NeuroflowN
{
    class OCLDataArray : public DataArray
	{
        bool isConst;
        OCLIntCtxSPtrT ctx;
		OCLBuffer1 buffer;

	public:
		OCLDataArray(const OCLIntCtxSPtrT& ctx, unsigned size, float* values, bool isConst);
		OCLDataArray(const OCLIntCtxSPtrT& ctx, unsigned size, float fill);

        DeviceArrayType GetType() const
        {
            return DeviceArrayType::DataArray;
        }

		bool GetIsConst() const
		{
            return isConst;
		}

		const OCLBuffer1& GetBuffer() const
		{
            return buffer;
		}

        unsigned GetSize() const
        {
            return buffer.GetSize();
        }

		void Read(int sourceBeginIndex, int count, float* targetPtr, int targetBeginIndex, doneCallback done);
        void Write(float* sourceArray, int sourceBeginIndex, int count, int targetBeginIndex, doneCallback done);

	private:
		cl::Buffer CreateBuffer(const OCLIntCtxSPtrT& ctx, unsigned size, float* values);
		static cl::Buffer CreateBuffer(const OCLIntCtxSPtrT& ctx, unsigned size, float fill);

        void VerifyIsNotConst();
	};
}