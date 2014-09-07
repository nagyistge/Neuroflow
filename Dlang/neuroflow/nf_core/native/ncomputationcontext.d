import computationcontext;
import randomgenerator;
import ccinitpars;
import devicearraymanagement;
import dataarrayfactory;
import ndevicearraymanagement;
import ndataarrayfactory;

class NComputationContext : ComputationContext
{
    this(in wstring deviceHint, in CCInitPars initPars)
    {
        auto pars = initPars !is null ? initPars : new CCInitPars();
        _randomGenerator = new RandomGenerator(pars.randomSeed);
        _deviceArrayManagement = new NDeviceArrayManagement();
        _dataArrayFactory = new NDataArrayFactory();
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

    private RandomGenerator _randomGenerator;

    private NDeviceArrayManagement _deviceArrayManagement;

    private NDataArrayFactory _dataArrayFactory;
}