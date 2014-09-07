import devicearraymanagement;
import devicearray;
import devicearray2;
import devicearraypool;
import ndevicearray;
import ndevicearray2;
import ndevicearraypool;

class NDeviceArrayManagement : DeviceArrayManagement
{
    DeviceArray createArray(bool copyOptimized, size_t size)
    {
        return new NDeviceArray(size);
    }
    
    DeviceArray2 createArray2(bool copyOptimized, size_t rowSize, size_t colSize)
    {
        return new NDeviceArray2(rowSize, colSize);
    }

    void copy(DeviceArray from, size_t fromIndex, DeviceArray to, size_t toIndex, size_t size)
    {
        auto nf = cast(NDeviceArray)from;
        auto nt = cast(NDeviceArray)to;
        assert(nf);
        assert(nt);
        auto nfa = nf.array;
        auto nta = nt.array;
        assert(nfa);
        assert(nta);
        assert(&nfa[0] != &nta[0]);
        nta[toIndex .. toIndex + size] = nfa[fromIndex .. fromIndex + size];
        assert(nfa[fromIndex] == nta[toIndex]);
        assert(nfa[fromIndex + size - 1] == nta[toIndex + size - 1]);
    }
    
    DeviceArrayPool createPool(bool copyOptimized)
    {
        return new NDeviceArrayPool();
    }
}