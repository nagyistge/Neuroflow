#pragma once
#include "nfobject.h"
#include <memory>
#include <string>
#include <sstream>
#include "OCL.h"

#define DEFINE_OCL_PROGRAM(p,s) p.Add(#s);

namespace NeuroflowN
{
	class OCLProgramBuilder : public NfObject
	{
		cl::Context clContext;
		cl::Device clDevice;
		std::stringstream programBuilder;

	public:
		OCLProgramBuilder(const cl::Context& clContext, const cl::Device& clDevice);

		void Add(const std::string& programString);

		cl::Program Compile();

    private:
        std::string CreateNumberedVersions(const std::string& prg);
	};
}
