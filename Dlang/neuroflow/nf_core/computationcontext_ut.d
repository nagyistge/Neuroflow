import computationcontextfactory;
import nfdefs;
import std.stdio;

unittest
{
    void enumDevices(in wstring contextName)
    {
        auto devices = ComputationContextFactory.instance.getAvailableDevices(NativeContext);
        assert(devices !is null && devices.length > 0, "Devices not found.");

        writefln("Devices of %s:", contextName);
        foreach (d; devices)
        {
            writefln("\tId: %s, Version: %s, Name: %s, Platform: %s", d.id, d.ver, d.name, d.platform);
            auto ctx = ComputationContextFactory.instance.createContext(contextName, d.id);
            assert(ctx.dataArrayFactory);
            assert(ctx.deviceArrayManagement);
            assert(ctx.randomGenerator);
            assert(ctx.utils);
            assert(ctx.deviceInfo.id == d.id);
            assert(ctx.deviceInfo.ver == d.ver);
            assert(ctx.deviceInfo.name == d.name);
            assert(ctx.deviceInfo.platform == d.platform);
        }
    }

    assert(ComputationContextFactory.instance !is null, "ComputationContextFactory.instance is null");
    
    // Native devices:
    enumDevices(NativeContext);
}
