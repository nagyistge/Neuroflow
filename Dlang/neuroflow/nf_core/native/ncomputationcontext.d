import computationcontext;
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
        _info = info;
        _deviceArrayManagement = new NDeviceArrayManagement();
        _dataArrayFactory = new NDataArrayFactory();
        _utils = new NUtils();
    }

    @property override immutable(DeviceInfo) deviceInfo()
    {
        return _info;
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

    private NDeviceArrayManagement _deviceArrayManagement;

    private NDataArrayFactory _dataArrayFactory;

    private NUtils _utils;
}