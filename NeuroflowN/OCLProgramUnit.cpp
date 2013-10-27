#include "stdafx.h"
#include "OCLProgramUnit.h"
#include "OCLIntCtx.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLProgramUnit::OCLProgramUnit(const OCLIntCtxSPtrT& ctx, const std::string& name) :
    ctx(ctx),
    name(name)
{
}

void OCLProgramUnit::Using(const std::shared_ptr<OCLProgramUnit>& baseUnit)
{
	if (baseUnit != null && find(baseUnits.cbegin(), baseUnits.cend(), baseUnit) == baseUnits.cend())
	{
		baseUnits.push_back(baseUnit);
	}
}

void OCLProgramUnit::AddCode(const std::string code)
{
	if (find(code.cbegin(), code.cend(), '$') != code.cend())
	{
		auto prg = CreateNumberedVersions(code);
		codeBuilder << prg << endl;
	}
	else
	{
		codeBuilder << code << endl;
	}
}

std::string OCLProgramUnit::CreateNumberedVersions(const std::string& prg)
{
	stringstream result;
	unsigned v = 1;
	do
	{
		if (v != 1) result << endl;
		string p = prg, vstr = to_string(v);
		boost::replace_all(p, "$$", vstr);
		if (v == 1)
		{
			boost::replace_all(p, "$", "");
		}
		else
		{
			boost::replace_all(p, "$", vstr);
		}
		result << p;
		v <<= 1;
	} while (v <= 16);
	return result.str();
}

std::vector<std::string> OCLProgramUnit::GetBaseHeaderNames()
{
    vector<string> names;
    for (auto& bu : baseUnits) names.push_back(bu->GetName());
    return move(names);
}

std::vector<cl::Program> OCLProgramUnit::GetBasePrograms()
{
    vector<Program> programs;
    for (auto& bu : baseUnits) programs.push_back(bu->GetProgram());
    return move(programs);
}

std::string OCLProgramUnit::GetCode()
{
    auto headers = GetBaseHeaderNames();
    stringstream ss;
    for (auto& hn : headers)
    {
        ss << "\n#include \"" << hn << "\"\n";
    }
    ss << codeBuilder.str();
    return ss.str();
}

cl::Program OCLProgramUnit::GetProgram()
{
    if (program() == null) program = Compile();
    return program;
}

cl::Program OCLProgramUnit::Compile()
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

    auto ver = toAlphanumeric(ctx->GetVersion());

    wstringstream fns;
    fns << wstring(exePath);
    fns << "\\";
    fns << "kernels\\";

    CreateDirectory(fns.str().c_str(), null);

    fns << ver;
    fns << "\\";

    CreateDirectory(fns.str().c_str(), null);

    fns << toAlphanumeric(name);
    fns << '_';
    fns << toAlphanumeric(ctx->GetDeviceInfo().ID);
    fns << ".bin";
    wstring fn = fns.str();

    vector<char> bin;

    auto fi = ifstream(fn.c_str(), ios::binary);
    if (fi.good())
    {
        bin = vector<char>(istreambuf_iterator<char>(fi), istreambuf_iterator<char>());
    }
    else
    {
        auto prog = CreateProgramAndBinary(bin);
        auto fo = ofstream(fn.c_str(), ios::binary);
        fo.write((char*)&bin[0], bin.size());
        fo.flush();
        return prog;
    }

    return CreateProgram(bin);
}

cl::Program OCLProgramUnit::CreateProgramAndBinary(std::vector<char>& bin)
{
    auto p = Program(ctx->GetContext(), GetCode().c_str(), false);
    Build(p, false);

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

cl::Program OCLProgramUnit::CreateProgram(const std::vector<char>& bin)
{
    Program::Binaries bins;
    bins.emplace_back(&bin[0], bin.size());
    auto p = Program(ctx->GetContext(), vector<Device>(1, ctx->GetDevice()), bins);
    Build(p, true);
    return p;
}

void OCLProgramUnit::Build(cl::Program& p, bool fromBin)
{
    try
    {
        if (!fromBin)
        {
            auto bnames = GetBaseHeaderNames();
            auto bprgs = GetBasePrograms();
            if (bnames.size() > 0)
            {
                assert(bprgs.size() == bnames.size());
                vector<cl_program> vprgs;
                vector<const char*> vnames;
                for (unsigned i = 0; i < bprgs.size(); i++)
                {
                    vprgs.push_back(bprgs[i]());
                    vnames.push_back(bnames[i].c_str());
                }
                clCompileProgram(
                    p(),
                    0, null,
                    null,
                    vprgs.size(),
                    &vprgs[0],
                    &vnames[0],
                    null, null);
            }
        }
        p.build(vector<Device>(1, ctx->GetDevice()));
    }
    catch (Error&)
    {
        auto info = p.getBuildInfo<CL_PROGRAM_BUILD_LOG>(ctx->GetDevice());
        info = string("\nOPENCL BUILD FAILED:\n") + info;
        throw_logic_error(info.c_str());
    }
}
