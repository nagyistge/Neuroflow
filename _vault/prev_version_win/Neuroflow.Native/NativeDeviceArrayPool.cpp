#include "stdafx.h"
#include "NativeDeviceArrayPool.h"
#include "NativeDeviceArray.h"
#include "NativeDeviceArray2.h"
#include "NativeException.h"

using namespace Neuroflow;
using namespace std;

IDeviceArray^ NativeDeviceArrayPool::CreateArray(int size)
{
    try
    {
        auto pArray = Ptr->CreateArray(size);
        try
        {
            return gcnew NativeDeviceArray(pArray);
        }
        catch (...)
        {
            delete pArray;
            throw;
        }
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

IDeviceArray2^ NativeDeviceArrayPool::CreateArray2(int rowSize, int colSize)
{
    try
    {
        auto pArray = Ptr->CreateArray2(rowSize, colSize);
        try
        {
            return gcnew NativeDeviceArray2(pArray);
        }
        catch (...)
        {
            delete pArray;
            throw;
        }
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

void NativeDeviceArrayPool::Allocate()
{
    try
    {
        Ptr->Allocate();
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}

void NativeDeviceArrayPool::Zero()
{
    try
    {
        Ptr->Zero();
    }
    catch (exception& ex)
    {
        throw gcnew NativeException(ex);
    }
}
