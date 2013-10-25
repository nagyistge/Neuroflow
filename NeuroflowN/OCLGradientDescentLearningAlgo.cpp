#include "stdafx.h"
#include "OCLGradientDescentLearningAlgo.h"
#include "OCLComputeGradientDescent.h"
#include "NNMetadata.h"
#include "OCLDeviceArrayManagement.h"
#include "OCLError.h"

using namespace std;
using namespace cl;
using namespace NeuroflowN;

OCLGradientDescentLearningAlgo::OCLGradientDescentLearningAlgo(const OCLIntCtxSPtrT& ctx, const OCLVaultSPtrT& vault, const std::shared_ptr<GradientDescentLearningRule>& rule, const TrainingNodeVecT& nodes) :
    ctx(ctx),
    LearningAlgo(rule, nodes),
	compute(ctx, vault)
{
    auto daMan = OCLDeviceArrayManagement(ctx);
    int eidx = 0;
    for (auto& node : nodes)
    {
        auto weights = node.Weights;
        auto gradients = node.Gradients;
        auto gradientSums = node.GradientSums;

        assert(weights.size() > 0);
        assert(rule->WeightUpdateMode == WeightUpdateMode::Offline || gradients && weights.size() == gradients->size());
        assert(rule->WeightUpdateMode == WeightUpdateMode::Online || gradientSums && weights.size() == gradientSums->size());

        for (unsigned i = 0; i < weights.size(); i++)
        {
            auto& wa = ctx->ToBuffer1(weights[i]);

            if (rule->WeightUpdateMode == WeightUpdateMode::Online)
            {
                auto& ga = ctx->ToBuffer1((*gradients)[i]);

                assert(wa.GetSize() == ga.GetSize());

                auto lua = OCLBuffer1SPtrT((OCLBuffer1 *) daMan.CreateArray(false, wa.GetSize()));
                gdKernelExecs.emplace_back();

                gdOnlineCode.push_back(
                    [=]()
                    {
                        auto& exec = gdKernelExecs[eidx];

                        compute.UpdateWeightsOnline(
                            exec,
                            lua->GetCLBuffer(),
                            wa,
                            ga,
                            rule->LearningRate,
                            rule->Momentum,
                            rule->Smoothing);
                    });
            }
            else
            {
                auto& ga = ctx->ToBuffer1((*gradientSums)[i]);

                assert(wa.GetSize() == ga.GetSize());

                auto lua = OCLBuffer1SPtrT((OCLBuffer1 *)daMan.CreateArray(false, wa.GetSize()));
                gdKernelExecs.emplace_back();

                gdOfflineCode.push_back(
                    [=](int iterationCount)
                    {
                        auto& exec = gdKernelExecs[eidx];

                        compute.UpdateWeightsOffline(
                            exec,
                            lua->GetCLBuffer(),
                            wa,
                            ga,
                            iterationCount,
                            rule->LearningRate,
                            rule->Momentum,
                            rule->Smoothing);
                    });
            }

            eidx++;
        }
    }
}

LearningAlgoIterationType OCLGradientDescentLearningAlgo::GetIterationTypes()
{
    return rule->WeightUpdateMode == WeightUpdateMode::Online ? LearningAlgoIterationType::SupervisedOnline : LearningAlgoIterationType::SupervisedOffline;
}

void OCLGradientDescentLearningAlgo::Run(int iterationCount, IDeviceArray* error)
{
    if (rule->WeightUpdateMode == WeightUpdateMode::Online)
    {
        try
        {
            for (auto& c : gdOnlineCode) c();
        }
        catch (exception& ex)
        {
            throw as_ocl_error(ex);
        }
    }
    else
    {
        try
        {
            for (auto& c : gdOfflineCode) c(iterationCount);
        }
        catch (exception& ex)
        {
            throw as_ocl_error(ex);
        }
    }
}