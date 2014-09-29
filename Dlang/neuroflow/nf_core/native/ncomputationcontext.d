import computationcontext;
import ccinitpars;
import devicearraymanagement;
import dataarrayfactory;
import ndevicearraymanagement;
import ndataarrayfactory;
import nutils;
import utils;
import deviceinfo;
import ncomputeactivation;
import computeactivation;
import nlearningimplfactory;
import learningimplfactory;

class NComputationContext : ComputationContext
{
    this(immutable DeviceInfo info, in CCInitPars initPars)
    {
        _info = info;
        _deviceArrayManagement = new NDeviceArrayManagement();
        _dataArrayFactory = new NDataArrayFactory();
        _utils = new NUtils();
    }

    @property immutable(DeviceInfo) deviceInfo()
    {
        return _info;
    }
    
    @property DeviceArrayManagement deviceArrayManagement()
    {
        return _deviceArrayManagement;
    }
    
    @property DataArrayFactory dataArrayFactory()
    {
        return _dataArrayFactory;
    }

    @property Utils utils()
    {
        return _utils;
    }

	@property ComputeActivation computeActivation()
	{
		return _computeActivation;
	}

	@property LearningImplFactory learningImplFactory()
	{
		return _learningImplFactory;
	}

    immutable DeviceInfo _info;

    private NDeviceArrayManagement _deviceArrayManagement = new NDeviceArrayManagement();

    private NDataArrayFactory _dataArrayFactory = new NDataArrayFactory();

    private NUtils _utils = new NUtils();

	private NComputeActivation _computeActivation = new NComputeActivation();

	private NLearningImplFactory _learningImplFactory = new NLearningImplFactory();
}