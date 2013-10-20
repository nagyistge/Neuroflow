#pragma once

#include "Typedefs.h"
#include "IDeviceArray.h"
#include <vector>
#include <functional>
#include "Error.h"

namespace NeuroflowN
{
    class DataArray : public IDeviceArray
    {
    public:
        virtual bool GetIsConst() const = 0;

		virtual void Read(int sourceBeginIndex, int count, float* targetPtr, int targetBeginIndex, doneCallback done)
		{
			throw_logic_error("Read is not implemented.");
		}

        virtual void Write(float* sourceArray, int sourceBeginIndex, int count, int targetBeginIndex, doneCallback done) = 0;
    };
}
