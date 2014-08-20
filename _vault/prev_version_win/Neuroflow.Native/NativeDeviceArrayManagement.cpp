#include "stdafx.h"
#include "NativeDeviceArrayManagement.h"
#include "IDeviceArrayManagement.h"
#include "NativeDeviceArray.h"
#include "NativeDeviceArray2.h"
#include "MUtil.h"
#include "NativeException.h"
#include "NativeDeviceArrayPool.h"

using namespace std;
using namespace Neuroflow;
using namespace Neuroflow::NeuralNetworks;

IDeviceArray^ NativeDeviceArrayManagement::CreateArray(bool copyOptimized, int size)
{
    try
    {
        return gcnew NativeDeviceArray(Ptr->CreateArray(copyOptimized, size));
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

IDeviceArray2^ NativeDeviceArrayManagement::CreateArray2(bool copyOptimized, int rowSize, int colSize)
{
    try
    {
        return gcnew NativeDeviceArray2(Ptr->CreateArray2(copyOptimized, rowSize, colSize));
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

void NativeDeviceArrayManagement::Copy(IDeviceArray^ from, int fromIndex, IDeviceArray^ to, int toIndex, int size)
{
    try
    {
        Ptr->Copy(ToNative(from), fromIndex, ToNative(to), toIndex, size);
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

IDeviceArrayPool^ NativeDeviceArrayManagement::CreatePool()
{
    try
    {
        return gcnew NativeDeviceArrayPool(Ptr->CreatePool());
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}