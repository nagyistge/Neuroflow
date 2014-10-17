#pragma once

#include <string>
#include <vector>

namespace NeuroflowN
{
	class OCLVectorKernelName
	{
		std::vector<std::string> names;

	public:
		OCLVectorKernelName(const std::string& name);

		const std::string& operator()(unsigned vectorSize) const;
	};
}