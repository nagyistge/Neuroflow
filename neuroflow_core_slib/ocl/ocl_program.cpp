#include "../stdafx.h"
#include "ocl_program.h"
#include "ocl_computation_context.h"
#include "../device_info.h"

USING
using namespace boost::filesystem;

ocl_program::ocl_program(const ocl_computation_context_wptr& context, const std::wstring& name) :
ocl_program_unit(context, name)
{
}

cl::Kernel ocl_program::create_kernel(const std::string name)
{
    return cl::Kernel(get_or_create_program(), name.c_str());
}

const cl::Program& ocl_program::get_or_create_program()
{
    if (program() == null) program = compile();
    return program;
}

cl::Program ocl_program::compile()
{
    auto ctx = lock_context();

    auto toAlphanumeric = [](const wstring& str)
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

    auto ver = toAlphanumeric(version());

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

    boost::thread([=]()
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

    fns << toAlphanumeric(name());
    fns << '_';
    fns << toAlphanumeric(ctx->device_info().id());
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
        auto prog = create_program_and_binary(bin);
        auto fo = ofstream(fn.c_str(), ios::binary);
        fo.write((char*)&bin[0], bin.size());
        fo.flush();
        return prog;
    }

    return create_program(bin);
}

cl::Program ocl_program::create_program_and_binary(std::vector<char>& bin)
{
    auto ctx = lock_context();
    auto code = this->code();
#if _DEBUG
    source = code;
#endif
    auto p = cl::Program(ctx->cl_context(), code.c_str(), false);
    build(p);

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

cl::Program ocl_program::create_program(const std::vector<char>& bin)
{
    auto ctx = lock_context();

    cl::Program::Binaries bins;
    bins.emplace_back(&bin[0], bin.size());
    auto p = cl::Program(ctx->cl_context(), vector<cl::Device>(1, ctx->cl_device()), bins);
    build(p);
    return p;
}

void ocl_program::build(cl::Program& program)
{
    auto ctx = lock_context();

    try
    {
        program.build(vector<cl::Device>(1, ctx->cl_device()), "-cl-fast-relaxed-math");
    }
    catch (cl::Error&)
    {
        auto info = program.getBuildInfo<CL_PROGRAM_BUILD_LOG>(ctx->cl_device());
        info = string("\nOPENCL BUILD FAILED:\n") + info;
        throw_logic_error(info.c_str());
    }
}
