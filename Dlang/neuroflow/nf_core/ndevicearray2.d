import devicearray2;
import ndevicearray;
import ndevicearraypool;

class NDeviceArray2 : NDeviceArray, DeviceArray2
{
    this(size_t size1, size_t size2)
    {
        super(size1 * size2);
        _size1 = size1;
        _size2 = size2;
    }

    this(NDeviceArrayPool pool, size_t beginIndex, size_t size1, size_t size2)
    {
        assert(pool);

        super(pool, beginIndex, size1 * size2);
        _size1 = size1;
        _size2 = size2;
    }

	@property void* deviceArray2Ptr()
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

    @property size_t size1()
    {
        return _size1;
    }
    
    @property size_t size2()
    {
        return _size2;
    }

    private size_t _size1, _size2;
}