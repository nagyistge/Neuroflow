import deviceinfo;
import ccfactoryadapter;
import std.exception;
import std.string;
import nccfactoryadapter;
public import computationcontext;
public import ccinitpars;

class ComputationContextFactory
{
    synchronized static this()
    {
        _instance = new ComputationContextFactory();
        _instance.registerType(NativeContext, new NCCFactoryAdapter());
    }

    @property static auto instance()
    {
        return _instance;
    }

    immutable(DeviceInfo[]) getAvailableDevices(in wstring typeId) 
    {
        auto adapter = findAdapter(typeId);
        return adapter.getAvailableDevices();
    }

    void registerType(in wstring typeId, CCFactoryAdapter adapter)
    {
        _adapters[typeId] = adapter;
    }

    ComputationContext createContext(in wstring typeId, in wstring deviceHint = "", in CCInitPars initPars = null)
    {
        auto adapter = findAdapter(typeId);
        return adapter.createContext(deviceHint, initPars);
    }

    private auto findAdapter(in wstring typeId)
    out (result)
    {
        assert(result);
    }
    body
    {
        auto adapter = _adapters.get(typeId, null);
        enforce(adapter, format("Device type '%s' is not registered.", typeId));
        return adapter;
    }

    private CCFactoryAdapter[wstring] _adapters;

    private static synchronized ComputationContextFactory _instance;
}