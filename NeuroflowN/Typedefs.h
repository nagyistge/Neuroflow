#pragma once

#include <vector>
#include <memory>
#include <functional>
#include <list>

#define null nullptr

namespace NeuroflowN
{
	struct LayerBehavior;
    struct LearningBehavior;
	struct DeviceInfo;
    struct TrainingNode;

    class NfObject;
	class DataArray;
    class DataArrayFactory;
    class IDeviceArray;
    class IDeviceArray2;
    class IDeviceArrayManagement;
    class IVectorUtils;
    class ILearningAlgo;
    class ILearningAlgoFactory;
    class IComputeActivation;
    class IMultilayerPerceptronAdapter;

    typedef std::shared_ptr<NfObject> NfObjectSPtrT;

    typedef std::shared_ptr<LearningBehavior> LearningBehaviorSPtrT;

	typedef std::function<void(std::exception* ex)> doneCallback;
	typedef std::function<void(float* pSource, std::exception* ex)> floatPtrDoneCallback;

	typedef std::vector<std::function<void()>> codeVecT;

    typedef std::tuple<DataArray*, DataArray*, DataArray*> SupervisedSampleEntryT;
    typedef std::list<SupervisedSampleEntryT> SupervisedSampleT;
    typedef std::list<SupervisedSampleT> SupervisedBatchT;

	typedef std::vector<DeviceInfo> DeviceInfoVecT;

    typedef std::vector<IDeviceArray*> DeviceArrayVecT;
    typedef std::vector<IDeviceArray2*> DeviceArray2VecT;
    typedef std::vector<std::function<IDeviceArray*()>> DeviceArrayFVecT;
}