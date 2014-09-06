import ccfactoryadapter;
import deviceinfo;
import computationcontext;
import ccinitpars;
import ncomputationcontext;

class NCCFactoryAdapter : CCFactoryAdapter
{
    immutable(DeviceInfo[]) getAvailableDevices()
    {
        return _devices;
    }
    
    ComputationContext createContext(in wstring deviceHint, in CCInitPars initPars)
    {
        return new NComputationContext(deviceHint, initPars);
    }

    private static shared immutable(DeviceInfo[]) _devices = 
    [ 
        DeviceInfo("NativeST", "1.0", "Native Single Threaded", "x86/x64"), 
        DeviceInfo("NativeMT", "1.0", "Native Multi Threaded", "x86/x64") 
    ];
}