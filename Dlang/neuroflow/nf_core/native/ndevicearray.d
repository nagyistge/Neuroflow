import devicearray;
import ndevicearraypool;

class NDeviceArray : DeviceArray
{
    this(float[] internalArray)
    {
        assert(internalArray);
        _internalArray = internalArray;
    }

    this(size_t arraySize)
    {
        _internalArray.length = (_arraySize = arraySize);
    }

    this(NDeviceArrayPool pool, size_t beginIndex, size_t arraySize)
    {
        assert(pool);
        _pool = pool;
        _beginIndex = beginIndex;
        _arraySize = arraySize;
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

    private float[] _internalArray;

    private size_t _arraySize, _beginIndex;

    private NDeviceArrayPool _pool;
}