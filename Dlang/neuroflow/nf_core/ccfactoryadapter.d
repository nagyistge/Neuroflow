import deviceinfo;
import computationcontext;
import ccinitpars;

interface CCFactoryAdapter
{
    immutable(DeviceInfo[]) getAvailableDevices();

    ComputationContext createContext(in wstring deviceHint, in CCInitPars initPars);
}