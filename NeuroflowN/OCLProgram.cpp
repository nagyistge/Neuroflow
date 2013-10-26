#include "stdafx.h"
#include "OCLProgram.h"
#include "OCLIntCtx.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLProgram::OCLProgram(const OCLIntCtxSPtrT& ctx, const std::string& name) :
	OCLProgramUnit(ctx),
	name(name)
{
}

cl::Program OCLProgram::GetProgram()
{
	if (program() == null) program = Compile();
	return program;
}

cl::Program OCLProgram::Compile()
{
	auto toAlphanumeric = [](const string& str)
	{
		wstringstream result;
		for (auto ch : str)
		{
			if ((ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')) result << ch;
		}
		return result.str();
	};

	TCHAR exePath[MAX_PATH];
	GetCurrentDirectory(MAX_PATH, exePath);

	wstringstream fns;
	fns << wstring(exePath);
	fns << "\\";
	fns << "kernels\\";

	CreateDirectory(fns.str().c_str(), null);

	fns << toAlphanumeric(name);
	fns << '_';
	fns << toAlphanumeric(ctx->GetDeviceInfo().ID);
	fns << '_';
	fns << ctx->GetVersion().c_str();
	fns << ".bin";
	wstring fn = fns.str();

	vector<char> bin;
	
	auto fi = ifstream(fn.c_str(), ios::binary);
	try
	{
		if (fi.good())
		{
			bin = vector<char>(istreambuf_iterator<char>(fi), istreambuf_iterator<char>());
		}
		else
		{
			auto prog = CreateProgramAndBinary(bin);
			auto fo = ofstream(fn.c_str(), ios::binary);
			try
			{
				fo.write((char*)&bin[0], bin.size());
				fo.flush();
			}
			catch (...)
			{
				fo.close();
				throw;
			}
			fo.close();
			return prog;
		}
	}
	catch (...)
	{
		fi.close();
		throw;
	}
	fi.close();

	return CreateProgram(bin);
}

cl::Program OCLProgram::CreateProgramAndBinary(std::vector<char>& bin)
{
	auto p = Program(ctx->GetContext(), GetCode().c_str(), false);
	try
	{
		p.build(vector<Device>(1, ctx->GetDevice()));
	}
	catch (Error&)
	{
		auto info = p.getBuildInfo<CL_PROGRAM_BUILD_LOG>(ctx->GetDevice());
		info = string("\nOPENCL BUILD FAILED:\n") + info;
		throw_logic_error(info.c_str());
	}

	auto s = p.getInfo<CL_PROGRAM_BINARY_SIZES>();
	assert(s.size() == 1);
	auto b = p.getInfo<CL_PROGRAM_BINARIES>();
	assert(b.size() == 1);
	try
	{
		bin.resize(s[0]);
		memcpy(&bin[0], b[0], s[0]);
	}
	catch (...)
	{
		delete[] b[0];
		throw;
	}
	delete[] b[0];

	return p;
}

cl::Program OCLProgram::CreateProgram(const std::vector<char>& bin)
{
	Program::Binaries bins;
	bins.emplace_back(&bin[0], bin.size());
	return Program(ctx->GetContext(), vector<Device>(1, ctx->GetDevice()), bins);
}