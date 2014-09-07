import randomgenerator;
import devicearraymanagement;
import dataarrayfactory;
import ccinitpars;
import utils;

class ComputationContext
{
    @property abstract RandomGenerator randomGenerator();

    @property abstract DeviceArrayManagement deviceArrayManagement();

    @property abstract DataArrayFactory dataArrayFactory();

    @property abstract Utils utils();
}