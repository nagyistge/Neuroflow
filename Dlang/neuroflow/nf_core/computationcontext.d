import randomgenerator;
import devicearraymanagement;
import dataarrayfactory;
import ccinitpars;
import utils;
import deviceinfo;

class ComputationContext
{
    @property abstract immutable(DeviceInfo) deviceInfo();

    @property abstract RandomGenerator randomGenerator();

    @property abstract DeviceArrayManagement deviceArrayManagement();

    @property abstract DataArrayFactory dataArrayFactory();

    @property abstract Utils utils();
}