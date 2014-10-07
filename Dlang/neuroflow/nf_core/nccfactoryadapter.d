import ccfactoryadapter;
import deviceinfo;
import computationcontext;
import ccinitpars;
import ncomputationcontext;
import std.string;
import std.exception;

class NCCFactoryAdapter : CCFactoryAdapter
{
    immutable(DeviceInfo[]) getAvailableDevices()
    {
        return _devices;
    }
    
    ComputationContext createContext(in wstring deviceHint, in CCInitPars initPars)
    {
        auto dhLower = toLower(strip(deviceHint));
        if (dhLower == "") return new NComputationContext(_devices[0], initPars);
        foreach (di; _devices)
        {
            if (toLower(di.id) == dhLower)
            {
                return new NComputationContext(di, initPars);
            }
        }
        throw new Exception(format("Natvie device by hint '%s' not found.", deviceHint));
    }

    private static shared immutable(DeviceInfo[]) _devices = 
    [ 
        DeviceInfo("NativeST", "1.0", "Native Single Threaded", "x86/x64"), 
        DeviceInfo("NativeMT", "1.0", "Native Multi Threaded", "x86/x64") 
    ];
}