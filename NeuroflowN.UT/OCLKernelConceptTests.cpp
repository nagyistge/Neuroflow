#include "stdafx.h"
#include "CppUnitTest.h"
#include "OCLContextImpl.h"
#include "OCLKernelToExecute.h"
#include "OCLDataArrayFactory.h"
#include "OCLDeviceArrayManagement.h"
#include "OCLProgram.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace std;
using namespace NeuroflowN;
using namespace cl;
using namespace concurrency;

namespace NeuroflowNUT
{
    TEST_CLASS(OCLKernelConceptTests)
    {
    public:
        
        TEST_METHOD(ComputeGradinetsRTLR_SetGradients_Test)
        {
            try
            {
                mt19937 generator((random_device()() << 16) | random_device()());
                uniform_real_distribution<float> uniform_distribution(0, 100.0);
                auto randF = bind(uniform_distribution, ref(generator));
                auto ver = to_string(randF());

                const unsigned size = 255;

                OCLContextImpl ctxImpl("cpu", ver);
                auto ctx = ctxImpl.GetIntCtx();
                auto prg = make_shared<OCLProgram>(ctx, "Bubu1");

                prg->Using(ctxImpl.GetVault()->GetCommonCode());
                prg->Using(ctxImpl.GetVault()->GetReduceCode());

                ADD_OCL_CODE(prg,

                    kernel void ComputeGradientsRTLR_SetGradients(
                        global float* values
                        ,int size
                        ,local float* tmpGradients
                        ,global float* gradients
                        ,global float* gradientSums
                        ,int gradientsIndex)
                    {
                        int localSize = get_local_size(0);
                        int localId = get_local_id(0);

                        int block = size / localSize + (size % localSize != 0 ? 1 : 0);
                        int idx = localId * block;
                        int max = idx + block;
                        if (max > size) max = size;

                        while (idx <  max)
                        {
                            tmpGradients[localId] += values[idx];

                            idx++;
                        }

                        Reduce_Sum(tmpGradients);

                        if (localId == 0)
                        {
                            if (gradients != null) gradients[gradientsIndex] = tmpGradients[0];
                            if (gradientSums != null) gradientSums[gradientsIndex] += tmpGradients[0];
                        }
                    }
                );

                unsigned lsize = ctx->GetOptimalLocalSizeForOneWorkgroup(size, 1);
                auto daF = ctxImpl.GetDataArrayFactoryPtr();
                auto deF = ctxImpl.GetDeviceArrayManagementPtr();
                auto a = daF->Create(size, 1.0f);
                auto r = daF->Create(lsize, 0.0f);

                OCLKernelToExecute exec;

                auto init = [=](Kernel& kernel)
                {
                    int aidx = 0;
                    kernel.setArg(aidx++, ctx->ToBuffer1(a)->GetCLBuffer());
                    kernel.setArg(aidx++, size);
                    kernel.setArg(aidx++, lsize * sizeof(float), null);
                    kernel.setArg(aidx++, ctx->ToBuffer1(r)->GetCLBuffer());
                    kernel.setArg(aidx++, null);
                    kernel.setArg(aidx++, 1);
                };

                exec.Execute(
                    prg,
                    "ComputeGradientsRTLR_SetGradients",
                    1,
                    init,
                    lsize,
                    lsize);

                vector<float> result(lsize);
                task_completion_event<void> e;
                task<void> t(e);

                r->Read(0, result.size(), &result[0], 0,
                    [&](exception* ex)
                {
                    if (ex) e.set_exception(ex);
                    else e.set();
                });

                t.wait();

                delete a;
                delete r;
            }
            catch (exception& ex)
            {
                Logger::WriteMessage(ex.what());
                throw;
            }
        }

    };
}