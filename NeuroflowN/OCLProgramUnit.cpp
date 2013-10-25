#include "stdafx.h"
#include "OCLProgramUnit.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLProgramUnit::OCLProgramUnit(const OCLIntCtxSPtrT& ctx) :
	ctx(ctx)
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
		programBuilder << prg;
	}
	else
	{
		programBuilder << code;
	}
}

std::string OCLProgramUnit::CreateNumberedVersions(const std::string& prg)
{
	stringstream result;
	unsigned v = 1;
	do
	{
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
