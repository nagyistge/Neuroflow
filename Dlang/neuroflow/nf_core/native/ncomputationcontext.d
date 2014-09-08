import computationcontext;
import randomgenerator;
import ccinitpars;
import devicearraymanagement;
import dataarrayfactory;
import ndevicearraymanagement;
import ndataarrayfactory;
import nutils;
import utils;
import deviceinfo;

class NComputationContext : ComputationContext
{
    this(immutable DeviceInfo info, in CCInitPars initPars)
    {
        auto pars = initPars !is null ? initPars : new CCInitPars();
        _info = info;
        _randomGenerator = new RandomGenerator(pars.randomSeed);
        _deviceArrayManagement = new NDeviceArrayManagement();
        _dataArrayFactory = new NDataArrayFactory();
        _utils = new NUtils();
    }

    @property override immutable(DeviceInfo) deviceInfo()
    {
        return _info;
    }

    @property override RandomGenerator randomGenerator()
    {
        return _randomGenerator;
    }
    
    @property override DeviceArrayManagement deviceArrayManagement()
    {
        return _deviceArrayManagement;
    }
    
    @property override DataArrayFactory dataArrayFactory()
    {
        return _dataArrayFactory;
    }

    @property override Utils utils()
    {
        return _utils;
    }

    immutable DeviceInfo _info;

    private RandomGenerator _randomGenerator;

    private NDeviceArrayManagement _deviceArrayManagement;

    private NDataArrayFactory _dataArrayFactory;

    private NUtils _utils;
}