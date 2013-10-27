#include "stdafx.h"
#include "CppUnitTest.h"
#include "OCLContextImpl.h"
#include "OCLIntCtx.h"
#include "OCLProgram.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace std;
using namespace NeuroflowN;

namespace NeuroflowNUT
{		
	TEST_CLASS(OCLProgramTests)
	{
	public:
		
		TEST_METHOD(OCLProgramCompileTest)
		{
			auto ctx = OCLContextImpl("CPU", "UT 1.0");
			auto unit1 = make_shared<OCLProgramUnit>(ctx.GetIntCtx(), "unit.h");
			auto prg = make_shared<OCLProgram>(ctx.GetIntCtx(), "OCLProgramCompileTest");

			ADD_OCL_CODE(unit1,
				int$ Sum$(int$ v1, int$ v2)
				{
					return v1 + v2;
				}
			);

			ADD_OCL_CODE(prg,
				kernel void SumKernel$(global int$* result, global int$* values1, global int$* values2)
				{
					unsigned idx = get_global_id(0);
					result[idx] = Sum$(values1[idx], values2[idx]);
				}
			);

			prg->Using(unit1);

            auto kernel = prg->CreateKernel("SumKernel2");

			auto prg2 = make_shared<OCLProgram>(ctx.GetIntCtx(), "OCLProgramCompileTest2");

			ADD_OCL_CODE(prg2,
				kernel void SumKernel2$(global int$* result, global int$* values1, global int$* values2)
				{
					unsigned idx = get_global_id(0);
					result[idx] = Sum$(values1[idx], values2[idx]);
				}
			);

			prg2->Using(unit1);

            auto kernel2 = prg2->CreateKernel("SumKernel28");
		}

	};
}