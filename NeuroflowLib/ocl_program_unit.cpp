#include "stdafx.h"
#include "ocl_program_unit.h"

USING;

ocl_program_unit::ocl_program_unit(const ocl_computation_context_wptr& context, const std::wstring& name) :
ocl_contexted(context),
_name(name)
{
}

void ocl_program_unit::include(const ocl_program_unit_ptr& baseUnit)
{
    if (baseUnit != null && find(baseUnits.cbegin(), baseUnits.cend(), baseUnit) == baseUnits.cend())
    {
        baseUnits.push_back(baseUnit);
    }
}

void ocl_program_unit::add_code(const std::string code)
{
    if (find(code.cbegin(), code.cend(), '$') != code.cend())
    {
        auto prg = create_numbered_versions(code);
        codeBuilder << endl << prg << endl;
    }
    else
    {
        codeBuilder << endl << code << endl;
    }
}

std::string ocl_program_unit::code()
{
    list<stringstream*> codeBuilders;
    add_builders(codeBuilders);

    unordered_set<string> guard;
    stringstream code;
    for (auto& b : codeBuilders)
    {
        auto c = b->str();
        if (guard.find(c) == guard.end())
        {
            guard.insert(c);
            code << c;
        }
    }

    return code.str();
}

const std::wstring& ocl_program_unit::name() const
{
    return _name;
}

void ocl_program_unit::add_builders(std::list<std::stringstream*>& to)
{
    for (auto& u : baseUnits) u->add_builders(to);
    to.push_back(&codeBuilder);
}

std::string ocl_program_unit::create_numbered_versions(const std::string& prg)
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