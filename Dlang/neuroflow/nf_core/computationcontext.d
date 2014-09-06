import randomgenerator;
import devicearraymanagement;
import dataarrayfactory;
import ccinitpars;

class ComputationContext
{
    @property abstract RandomGenerator randomGenerator();

    @property abstract DeviceArrayManagement deviceArrayManagement();

    @property abstract DataArrayFactory dataArrayFactory();
}