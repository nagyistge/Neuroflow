#include "stdafx.h"
#include "NativeDataArray.h"
#include "Callbacks.h"
#include "NativeException.h"

using namespace System;
using namespace System::Threading::Tasks;
using namespace System::Runtime::InteropServices;

void Neuroflow::Data::NativeDataArray::ReadAsync(int sourceBeginIndex, int count, float* targetPtr, int targetBeginIndex, DataArrayCompletedCallback^ doneFunc)
{
	try
	{
		auto fp = Marshal::GetFunctionPointerForDelegate(doneFunc).ToPointer();

		dataArray->Read(sourceBeginIndex, count, targetPtr, targetBeginIndex, StandardCallback((StandardCallbackFnc*)fp));
	}
	catch (std::exception& ex)
	{
		throw gcnew Neuroflow::NativeException(ex);
	}
}

void Neuroflow::Data::NativeDataArray::WriteAsync(float* sourcePtr, int sourceBeginIndex, int count, int targetBeginIndex, DataArrayCompletedCallback^ doneFunc)
{
	try
	{
        auto fp = Marshal::GetFunctionPointerForDelegate(doneFunc).ToPointer();

		dataArray->Write(sourcePtr, sourceBeginIndex, count, targetBeginIndex, StandardCallback((StandardCallbackFnc*)fp));
	}
	catch (std::exception& ex)
	{
		throw gcnew Neuroflow::NativeException(ex);
	}
}
