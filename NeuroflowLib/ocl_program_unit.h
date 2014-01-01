#pragma once

#include "ocl_nfdev.h"
#include "ocl_contexted.h"

#define ADD_OCL_CODE(u, c) u->add_code(#c);

namespace nf
{
    struct ocl_program_unit : ocl_contexted
    {
        ocl_program_unit(const ocl_computation_context_wptr& context, const std::wstring& name);

        void using_base(const ocl_program_unit_ptr& baseUnit);
        void add_code(const std::string code);
        std::string code();
        const std::wstring& name() const;

    protected:
        void add_builders(std::list<std::stringstream*>& to);

    private:
        std::wstring _name;
        std::stringstream codeBuilder;
        std::list<ocl_program_unit_ptr> baseUnits;

        std::string create_numbered_versions(const std::string& prg);
    };
}