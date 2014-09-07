import computationcontext;
import randomgenerator;
import ccinitpars;
import devicearraymanagement;
import dataarrayfactory;
import ndevicearraymanagement;
import ndataarrayfactory;
import nutils;
import utils;

class NComputationContext : ComputationContext
{
    this(in wstring deviceHint, in CCInitPars initPars)
    {
        auto pars = initPars !is null ? initPars : new CCInitPars();
        _randomGenerator = new RandomGenerator(pars.randomSeed);
        _deviceArrayManagement = new NDeviceArrayManagement();
        _dataArrayFactory = new NDataArrayFactory();
        _utils = new NUtils();
    }

    @property override RandomGenerator randomGenerator()
    {
        return randomGenerator;
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

    private RandomGenerator _randomGenerator;

    private NDeviceArrayManagement _deviceArrayManagement;

    private NDataArrayFactory _dataArrayFactory;

    private NUtils _utils;
}