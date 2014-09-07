import computationcontextfactory;
import nfdefs;

unittest
{
    assert(ComputationContextFactory.instance !is null, "ComputationContextFactory.instance is null");
    
    // Enumerate native devices:
    auto devices = ComputationContextFactory.instance.getAvailableDevices(NativeContext);
    assert(devices !is null && devices.length == 2, "Devices not found.");
}
