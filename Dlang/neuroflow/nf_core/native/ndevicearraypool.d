import devicearraypool;
import devicearray;
import devicearray2;
import ndevicearray;
import ndevicearray2;
import std.exception;
import std.algorithm;

class NDeviceArrayPool : DeviceArrayPool
{
    @property bool isAllocated() const
    {
        return checkIfAllocated();
    }
    
    DeviceArray createArray(size_t size)
    {
        return new NDeviceArray(this, reserve(size), size);
    }
    
    DeviceArray2 createArray2(size_t rowSize, size_t colSize)
    {
        return new NDeviceArray2(this, reserve(rowSize * colSize), rowSize, colSize);
    }
    
    void allocate()
    {
        doAllocate();
    }

    void zero()
    {
        enforce(checkIfAllocated(), "Cannot zero out an unallocated pool.");
        fill(internalArray, 0.0f);
    }

    float[] makeSlice(size_t beginIndex, size_t length)
    {
        auto arr = array;
        assert(beginIndex < arr.length);
        assert(length - beginIndex < arr.length);
        return arr[beginIndex .. beginIndex + length];
    }

    @property float[] array()
    {
        doAllocate();
        assert(internalArray != null);
        assert(isAllocated());
        return internalArray;
    }

    private size_t endIndex = 0;

    private float[] internalArray;
    
    private bool checkIfAllocated() const
    {
        return internalArray !is null;
    }

    private void doAllocate()
    {
        enforce(endIndex != 0, "There is no allocated memory in the pool.");
        if (!checkIfAllocated())
        {
            internalArray.length = endIndex;
            fill(internalArray, 0.0f);
        }
    }

    private size_t reserve(size_t size)
    {
        enforce(!checkIfAllocated(), "Cannot reserve memory in an already allocated pool.");
        auto beginIndex = endIndex;
        endIndex += size;
        return beginIndex;
    }
}