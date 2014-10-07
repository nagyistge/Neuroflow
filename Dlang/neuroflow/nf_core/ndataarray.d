import dataarray;
import aliases;
import ndevicearray;

class NDataArray : NDeviceArray, DataArray
{
    this(float[] internalArray, bool isConst)
    {
        assert(internalArray);

        super(internalArray);
        _isConst = isConst;
    }

	@property void* dataArrayPtr()
	{
		return cast(void*)this;
	}

    override @property void* deviceArrayPtr()
    {
        return super.deviceArrayPtr();
    }

    override @property size_t size()
    {
        return super.size;
    }

    @property bool isConst() const
    {
        return _isConst;
    }
    
    void read(size_t sourceBeginIndex, size_t count, float* targetPtr, size_t targetBeginIndex)
    {
        assert(targetPtr);
        auto a = this.array;
        targetPtr[targetBeginIndex .. targetBeginIndex + count] = a[sourceBeginIndex .. sourceBeginIndex + count];
        assert(targetPtr[targetBeginIndex] == a[sourceBeginIndex]);
        assert(targetPtr[targetBeginIndex + count - 1] == a[sourceBeginIndex + count - 1]);
    }
    
    void write(in float* sourcePtr, in size_t sourceBeginIndex, size_t count, size_t targetBeginIndex)
    {
        assert(sourcePtr);
        auto a = this.array;
        a[targetBeginIndex .. targetBeginIndex + count] = sourcePtr[sourceBeginIndex .. sourceBeginIndex + count];
        assert(sourcePtr[sourceBeginIndex] == a[targetBeginIndex]);
        assert(sourcePtr[sourceBeginIndex + count - 1] == a[targetBeginIndex + count - 1]);
    }

    private bool _isConst;
}