import computationcontext;
import randomgenerator;
import ccinitpars;
import devicearraymanagement;
import dataarrayfactory;

class NComputationContext : ComputationContext
{
    this(in wstring deviceHint, in CCInitPars initPars)
    {
        auto pars = initPars !is null ? initPars : new CCInitPars();
        _randomGenerator = new RandomGenerator(pars.randomSeed);
    }

    @property override RandomGenerator randomGenerator()
    {
        assert(false, "TODO");
    }
    
    @property override DeviceArrayManagement deviceArrayManagement()
    {
        assert(false, "TODO");
    }
    
    @property override DataArrayFactory dataArrayFactory()
    {
        assert(false, "TODO");
    }

    RandomGenerator _randomGenerator;
}