#include "Stdafx.h"
#include "NativeVectorUtils.h"
#include "MUtil.h"
#include "NativeException.h"
#include "IVectorUtils.h"
#include "NativeDataArray.h"

using namespace std;
using namespace Neuroflow;
using namespace Neuroflow::Data;

void NativeVectorUtils::Zero(IDeviceArray^ deviceArray)
{
    try
    {
        vectorUtils->Zero(ToNative(deviceArray));
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

void NativeVectorUtils::RandomizeUniform(IDeviceArray^ values, float min, float max)
{ 
    try
    {
        vectorUtils->RandomizeUniform(ToNative(values), min, max);
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

void NativeVectorUtils::DoCalculateMSE(Data::SupervisedBatch^ batch, DataArray^ mseValues, int valueIndex)
{
    try
    {
        vectorUtils->CalculateMSE(ToNative(batch), ToNative(mseValues), (unsigned)valueIndex);
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}