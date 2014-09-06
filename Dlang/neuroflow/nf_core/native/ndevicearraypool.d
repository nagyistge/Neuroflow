import devicearraypool;
import devicearray;
import devicearray2;

class NDeviceArrayPool : DeviceArrayPool
{
    @property bool isAllocated() const
    {
        assert(false, "TODO");
    }
    
    DeviceArray createArray(size_t size)
    {
        assert(false, "TODO");
    }
    
    DeviceArray2 createArray2(size_t rowSize, size_t colSize)
    {
        assert(false, "TODO");
    }
    
    void allocate()
    {
        assert(false, "TODO");
    }
    
    void zero()
    {
        assert(false, "TODO");
    }

    float[] makeSlice(size_t beginIndex, size_t length)
    {
        assert(false, "TODO");
    }
}