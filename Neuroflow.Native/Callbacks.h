#pragma once

#include <exception>
#include "NativeException.h"
#include "Typedefs.h"
#include "MUtil.h"

namespace Neuroflow
{
	typedef void (__stdcall StandardCallbackFnc)(System::Exception^);

	class StandardCallback
	{
		StandardCallbackFnc* f;

	public:
		StandardCallback(StandardCallbackFnc* f) : f(f) { }
			
		void operator()(std::exception* ex)
		{
			if (ex == nullptr)
			{
				f(nullptr);
			}
			else
			{
				f(gcnew NativeException(*ex));
			}
		}
	};

    typedef IDeviceArray^ (__stdcall DeviceArrayFuncFnc)();

    class DeviceArrayFunc
    {
        DeviceArrayFuncFnc* f;

    public:
        DeviceArrayFunc(DeviceArrayFuncFnc* f) : f(f) { }

        NeuroflowN::IDeviceArray* operator()()
        {
            auto p = f();
            return ToNative(p);
        }
    };
}