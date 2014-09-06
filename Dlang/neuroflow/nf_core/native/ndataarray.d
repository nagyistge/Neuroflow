import dataarray;
import aliases;

class NDataArray : DataArray
{
    this(float[] internalArray, bool isConst)
    {
        assert(internalArray);

        _isConst = isConst;
        _internalArray = internalArray;
    }

    @property size_t size()
    {
        return _internalArray.length;
    }

    @property bool isConst() const
    {
        return _isConst;
    }
    
    void read(in DoneFunc doneCallback, size_t sourceBeginIndex, size_t count, float* targetPtr, size_t targetBeginIndex)
    {
        memcpy();
    }
    
    void write(in DoneFunc doneCallback, in float* sourceArray, in size_t sourceBeginIndex, size_t count, size_t targetBeginIndex)
    {
    }

    private bool _isConst;

    private float[] _internalArray;
}