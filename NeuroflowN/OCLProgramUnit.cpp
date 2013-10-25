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

std::string OCLProgramUnit::GetCode()
{
	list<stringstream*> codeBuilders;
	GetBuilders(codeBuilders);

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

void OCLProgramUnit::GetBuilders(std::list<std::stringstream*>& to)
{
	for (auto& u : baseUnits) u->GetBuilders(to);
	to.push_back(&codeBuilder);
}
