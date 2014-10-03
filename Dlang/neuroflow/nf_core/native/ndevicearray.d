public import devicearray;
import ndevicearraypool;
import std.algorithm;

class NDeviceArray : DeviceArray
{
    this(float[] internalArray)
    {
        assert(internalArray);
        _internalArray = internalArray;
        _arraySize = _internalArray.length;
    }

    this(size_t arraySize)
    {
        _internalArray.length = (_arraySize = arraySize);
        fill(_internalArray, 0.0f);
    }

    this(NDeviceArrayPool pool, size_t beginIndex, size_t arraySize)
    {
        assert(pool);
        _pool = pool;
        _beginIndex = beginIndex;
        _arraySize = arraySize;
    }

	@property void* deviceArrayPtr()
	{
		return cast(void*)this;
	}

    @property size_t size()
    {
        return _arraySize;
    }

    @property float[] array()
    out(result)
    {
        assert(result);
    }
    body
    {
        if (_internalArray is null && _pool !is null) _internalArray = _pool.makeSlice(_beginIndex, _arraySize);
        return _internalArray;
    }

    @property void* primaryPtr()
    {
        return _primaryPtr;
    }
    
    @property void primaryPtr(void* ptr)
    {
        assert(ptr);
        _primaryPtr = ptr;
    }
    
    @property void* secondaryPtr()
    {
        return _secondaryPtr;
    }
    
    @property void secondaryPtr(void* ptr)
    {
        assert(ptr);
        _secondaryPtr = ptr;
    }

    private float[] _internalArray;

    private size_t _arraySize, _beginIndex;

    private NDeviceArrayPool _pool;

    private void* _primaryPtr, _secondaryPtr;
}