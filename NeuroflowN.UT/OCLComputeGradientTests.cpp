#include "stdafx.h"
#include "CppUnitTest.h"
#include "OCLContextImpl.h"
#include "OCLIntCtx.h"
#include "DataArrayFactory.h"
#include "IVectorUtils.h"
#include "OCLComputeGradientDescent.h"

using namespace std;
using namespace NeuroflowN;
using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace concurrency;
using namespace boost::chrono;

namespace NeuroflowNUT
{
	TEST_CLASS(OCLComputeGradientTests)
	{
	public:
        
        BEGIN_TEST_METHOD_ATTRIBUTE(ComputeGradientsOnlineTest)
            TEST_METHOD_ATTRIBUTE(L"Native", L"OCL")
        END_TEST_METHOD_ATTRIBUTE()
        TEST_METHOD(ComputeGradientsOnlineTest)
        {
            try
            {
                const unsigned size = 4099;
                const unsigned count = 100000;

                auto ctx = OCLContextImpl("cpu", "UT 1.0");
                auto daF = ctx.GetDataArrayFactoryPtr();
                auto comp = OCLComputeGradientDescent(ctx.GetIntCtx(), ctx.GetVault());

                auto lastUpdates = daF->Create(size, 1.1f);
                auto weights = daF->Create(size, 1.1f);
                auto gradients = daF->Create(size, 1.1f);

                vector<float> r(size);
                auto exec = OCLKernelToExecute();

                task_completion_event<void> e;
                task<void> t(e);

                auto startOn = high_resolution_clock::now();

                for (unsigned i = 0; i < count; i++)
                {
                    comp.UpdateWeightsOnline(exec, ctx.GetIntCtx()->ToBuffer1(lastUpdates), ctx.GetIntCtx()->ToBuffer1(weights), ctx.GetIntCtx()->ToBuffer1(gradients), 0.01f, 0.01f, false);
                }

                weights->Read(0, size, &r[0], 0,
                    [&](exception* ex)
                {
                    if (ex) e.set_exception(ex);
                    else e.set();
                });

                t.wait();

                auto dur = duration_cast<duration<double, boost::milli>>(high_resolution_clock::now() - startOn);

                Logger::WriteMessage(to_string(dur).c_str());
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

	};
}