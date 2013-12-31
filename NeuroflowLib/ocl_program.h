#pragma once

#include "ocl_nfdev.h"
#include "ocl_program_unit.h"

namespace nf
{
    struct ocl_program : ocl_program_unit
    {
        ocl_program(const ocl_internal_context_ptr& context, const std::wstring& name);
        cl::Kernel create_kernel(const std::string name);

    private:
        cl::Program program;
#if _DEBUG
        std::string source;
#endif

        const cl::Program& get_or_create_program();
        cl::Program compile();
        cl::Program create_program_and_binary(std::vector<char>& bin);
        cl::Program create_program(const std::vector<char>& bin);
        inline void build(cl::Program& program);
    };
}