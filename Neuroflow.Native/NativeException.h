#pragma once

#include <exception>

namespace Neuroflow
{
	ref class NativeException : public System::Exception
	{
	public:
		NativeException(const std::exception& ex) :
			System::Exception(CreateMessage(ex))
		{
		}
	private:
		static System::String^ CreateMessage(const std::exception& ex)
		{
			std::stringstream msg;
			msg << ex.what() << "\n(Exception type: '" << typeid(ex).name() << "')";
			return gcnew System::String(msg.str().c_str());
		}
	};
}