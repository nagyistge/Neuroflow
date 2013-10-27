#include "stdafx.h"
#include "OCLProgram.h"
#include "OCLIntCtx.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;
using namespace boost::filesystem;

OCLProgram::OCLProgram(const OCLIntCtxSPtrT& ctx, const std::string& name) :
    OCLProgramUnit(ctx, name)
{
}

cl::Kernel OCLProgram::CreateKernel(const std::string name)
{
    return Kernel(GetProgram(), name.c_str());
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

    auto ver = toAlphanumeric(ctx->GetVersion());

    wstringstream fns;
    fns << wstring(exePath);
    fns << "\\";
    fns << "kernels\\";

    path kernelPath(fns.str());
    create_directory(kernelPath);

    fns << ver;
    fns << "\\";

    path verPath(fns.str());
    create_directory(verPath);

    concurrency::create_task([=]()
    {
        vector<path> del;
        for (auto p = directory_iterator(kernelPath); p != directory_iterator(); p++)
        {
            if (!equivalent(*p, verPath)) del.push_back(*p);
        }

        for (auto& p : del)
        {
            remove_all(p);
        }
    });    

    fns << toAlphanumeric(GetName());
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

cl::Program OCLProgram::CreateProgramAndBinary(std::vector<char>& bin)
{
    auto p = Program(ctx->GetContext(), GetCode().c_str(), false);
    Build(p);

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
    auto p = Program(ctx->GetContext(), vector<Device>(1, ctx->GetDevice()), bins);
    Build(p);
    return p;
}

void OCLProgram::Build(cl::Program& p)
{
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
}