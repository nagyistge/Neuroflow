// This is the main DLL file.

#include "stdafx.h"

#include "NativeDataArrayFactory.h"
#include "NativeDataArray.h"
#include "DataArrayFactory.h"
#include "NativeException.h"

using namespace System;
using namespace std;

Neuroflow::Data::DataArray^ Neuroflow::Data::NativeDataArrayFactory::DoCreate(int size, float fill)
{
	try
	{
		auto pArray = dataArrayFactory->Create(size, fill);
		try
		{
			return gcnew Neuroflow::Data::NativeDataArray(pArray);
		}
		catch (...)
		{
			delete pArray;
			throw;
		}
	}
	catch (std::exception& ex)
	{
		throw gcnew Neuroflow::NativeException(ex);
	}
}

Neuroflow::Data::DataArray^ Neuroflow::Data::NativeDataArrayFactory::DoCreate(array<float>^ array, int beginPos, int length)
{
	try
	{
		pin_ptr<float> p = &array[0];
		auto pArray = dataArrayFactory->Create(p, beginPos, length);
		try
		{
			return gcnew Neuroflow::Data::NativeDataArray(pArray);
		}
		catch (...)
		{
			delete pArray;
			throw;
		}
	}
	catch (std::exception& ex)
	{
		throw gcnew Neuroflow::NativeException(ex);
	}
}

Neuroflow::Data::DataArray^ Neuroflow::Data::NativeDataArrayFactory::DoCreateConst(array<float>^ array, int beginPos, int length)
{
	try
	{
		pin_ptr<float> p = &array[0];
		auto pArray = dataArrayFactory->CreateConst(p, beginPos, length);
		try
		{
			return gcnew Neuroflow::Data::NativeDataArray(pArray);
		}
		catch (...)
		{
			delete pArray;
			throw;
		}
	}
	catch (std::exception& ex)
	{
		throw gcnew Neuroflow::NativeException(ex);
	}
}
