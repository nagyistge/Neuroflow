#include "stdafx.h"
#include "CppUnitTest.h"
#include "OCLContextImpl.h"
#include "OCLIntCtx.h"
#include "DataArrayFactory.h"
#include "IVectorUtils.h"

using namespace std;
using namespace NeuroflowN;
using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace concurrency;
using namespace boost::chrono;

namespace NeuroflowNUT
{
	TEST_CLASS(OCLVectorUtilsTests)
	{
	public:
		
		TEST_METHOD(ZeroTest)
		{
            const unsigned size = 2048;
            const unsigned count = 100000;

            auto ctx = OCLContextImpl("GPU", "UT 1.0");
            auto daF = ctx.GetDataArrayFactoryPtr();
            auto vu = ctx.GetVectorUtilsPtr();
            auto a = daF->Create(size, 0.0f);
            vector<float> r;
            r.resize(size);

            task_completion_event<void> e;
            task<void> t(e);

            auto startOn = high_resolution_clock::now();

            for (unsigned i = 0; i < count; i++)
            {
                vu->Zero(a);
            }

            a->Read(0, size, &r[0], 0, 
            [&](exception* ex) 
            { 
                if (ex) e.set_exception(ex);
                else e.set(); 
            });

            t.wait();

            auto dur = duration_cast<duration<double, boost::milli>>(high_resolution_clock::now() - startOn);

            Logger::WriteMessage(to_string(dur).c_str());
		}

	};
}