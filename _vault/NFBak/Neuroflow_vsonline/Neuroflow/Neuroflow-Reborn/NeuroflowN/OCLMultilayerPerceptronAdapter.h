#pragma once

#include "IMultilayerPerceptronAdapter.h"
#include "OCLTypedefs.h"

namespace NeuroflowN
{
    class OCLMultilayerPerceptronAdapter : public IMultilayerPerceptronAdapter
    {
        OCLIntCtxSPtrT ctx;
        std::shared_ptr<IVectorUtils> vectorUtils;
        std::shared_ptr<IDeviceArrayManagement> deviceArrayManagement;
        std::shared_ptr<IComputeActivation> computeActivation;
        std::shared_ptr<ILearningAlgoFactory> learningAlgoFactory;

    public:
        OCLMultilayerPerceptronAdapter(const OCLIntCtxSPtrT& ctx, const std::shared_ptr<IVectorUtils>& vectorUtils, const std::shared_ptr<IDeviceArrayManagement>& deviceArrayManagement);

        IVectorUtils* GetVectorUtilsPtr() const
        {
            return vectorUtils.get();
        }

        IDeviceArrayManagement* GetDeviceArrayManagementPtr() const
        {
            return deviceArrayManagement.get();
        }

        IComputeActivation* GetComputeActivationPtr() const
        {
            return computeActivation.get();
        }

        ILearningAlgoFactory* GetLearningAlgoFactoryPtr() const
        {
            return learningAlgoFactory.get();
        }
    };
}