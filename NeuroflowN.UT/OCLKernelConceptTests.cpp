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

                const unsigned size = 129;

                OCLContextImpl ctxImpl("cpu", ver);
                auto ctx = ctxImpl.GetIntCtx();
                auto prg = make_shared<OCLProgram>(ctx, "Bubu1");

                prg->Using(ctxImpl.GetVault()->GetCommonCode());

                prg->AddCode("#define null 0");

                ADD_OCL_CODE(prg,

                    kernel void ComputeGradientsRTLR_SetGradients(
                        global float* values
                        ,int size
                        ,global int2* tmp
                        ,global float* gradients
                        ,global float* gradientSums
                        ,int gradientsIndex)
                    {
                        int globalSize = get_global_size(0);
                        int globalId = get_global_id(0);
                        int localSize = get_local_size(0);
                        int localId = get_local_id(0);
                        int groupId = get_group_id(0);
                        int groupCount = get_num_groups(0);
                        int idx;

                        if (globalId == 0)
                        {
                            // Set tmp to zero:
                            AtomAddG2(tmp, -(tmp[0]));
                            //barrier(CLK_GLOBAL_MEM_FENCE);
                        }
                        else if (groupId != 0)
                        {
                            // Wait until tmp is zero
                            int2 v = tmp[0];
                            while (!(v.hi == 0 && v.lo == 0)) v = tmp[0];
                        }

                        local int tmpSum[8];
                        if (localId < 8) tmpSum[localId] = 0;
                        barrier(CLK_LOCAL_MEM_FENCE);

                        idx = globalId;
                        while (idx < size)
                        {
                            // Global gradient calculated as:
                            float gradient = values[idx];

                            int tmpIdx = globalId % 8;
                            AtomAdd(&tmpSum[tmpIdx], convert_int_rte(gradient * D));

                            idx += globalSize;
                        }

                        barrier(CLK_LOCAL_MEM_FENCE);

                        if (localId == 0)
                        {
                            int2 v;
                            v.hi = tmpSum[0] + tmpSum[1] + tmpSum[2] + tmpSum[3] + tmpSum[4] + tmpSum[5] + tmpSum[6] + tmpSum[7];
                            v.lo = 1;
                            AtomAddG2(tmp, v);
                        }

                        if (globalId == 0)
                        {
                            int count = tmp[0].lo;
                            while (count < groupCount) count = tmp[0].lo;
                            float fg = convert_float(tmp[0].hi) / D;
                            if (gradients != null) gradients[gradientsIndex] = fg;
                            if (gradientSums != null) gradientSums[gradientsIndex] += fg;
                        }
                    }
                );

                unsigned gsize = ctx->GetOptimalGlobalSize(size, 1);
                auto daF = ctxImpl.GetDataArrayFactoryPtr();
                auto deF = ctxImpl.GetDeviceArrayManagementPtr();
                auto a = daF->Create(size, 1.0f);
                auto r = daF->Create(5, 0.0f);
                auto tmp = Buffer(ctx->GetContext(), 0, 1 * sizeof(cl_int2));

                OCLKernelToExecute exec;

                auto init = [=](Kernel& kernel)
                {
                    int aidx = 0;
                    kernel.setArg(aidx++, ctx->ToBuffer1(a).GetCLBuffer());
                    kernel.setArg(aidx++, size);
                    kernel.setArg(aidx++, tmp);
                    kernel.setArg(aidx++, ctx->ToBuffer1(r).GetCLBuffer());
                    kernel.setArg(aidx++, null);
                    kernel.setArg(aidx++, 1);
                };

                exec.Execute(
                    prg,
                    "ComputeGradientsRTLR_SetGradients",
                    1,
                    init,
                    gsize,
                    16);

                vector<float> result(5);
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