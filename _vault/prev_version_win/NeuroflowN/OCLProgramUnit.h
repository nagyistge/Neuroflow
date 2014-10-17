#pragma once

#include <list>
#include <string>
#include "OCLTypedefs.h"

#define ADD_OCL_CODE(u,c) u->AddCode(#c);

namespace NeuroflowN
{
    class OCLProgramUnit
    {
        std::string name;
        std::stringstream codeBuilder;
        std::list<OCLProgramUnitSPtrT> baseUnits;

    public:
        OCLProgramUnit(const OCLProgramUnit&) = delete;
        OCLProgramUnit(const OCLIntCtxSPtrT& ctx, const std::string& name);
        void Using(const std::shared_ptr<OCLProgramUnit>& baseUnit);
        void AddCode(const std::string code);
        std::string GetCode();

        const OCLIntCtxSPtrT& GetIntCtx() const
        {
            return ctx;
        }

        const std::string& GetName() const
        {
            return name;
        }

    protected:
        OCLIntCtxSPtrT ctx;

        void GetBuilders(std::list<std::stringstream*>& to);

    private:
        std::string CreateNumberedVersions(const std::string& prg);
    };
}