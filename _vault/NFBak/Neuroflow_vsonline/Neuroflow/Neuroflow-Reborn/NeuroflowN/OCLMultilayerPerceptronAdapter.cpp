#include "stdafx.h"
#include "OCLMultilayerPerceptronAdapter.h"
#include "OCLDeviceArrayManagement.h"
#include "OCLVectorUtils.h"
#include "OCLComputeActivation.h"
#include "OCLLearningAlgoFactory.h"

using namespace std;
using namespace NeuroflowN;

OCLMultilayerPerceptronAdapter::OCLMultilayerPerceptronAdapter(const OCLIntCtxSPtrT& ctx, const std::shared_ptr<IVectorUtils>& vectorUtils, const std::shared_ptr<IDeviceArrayManagement>& deviceArrayManagement) :
    ctx(ctx),
    vectorUtils(vectorUtils),
    deviceArrayManagement(deviceArrayManagement),
    computeActivation(make_shared<OCLComputeActivation>(ctx)),
    learningAlgoFactory(make_shared<OCLLearningAlgoFactory>(ctx))
{
}