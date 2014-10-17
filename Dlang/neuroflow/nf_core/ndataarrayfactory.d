import dataarrayfactory;
import dataarray;
import ndataarray;
import std.algorithm;

class NDataArrayFactory : DataArrayFactory
{
    DataArray create(size_t size, float fill = 0.0f)
    {
        float[] a;
        a.length = size;
        .fill(a, fill);

        return new NDataArray(a, false);
    }
    
    DataArray create(in float* values, size_t beginPos, size_t size)
    {
        return doCreate(values, beginPos, size, false);
    }
    
    DataArray createConst(in float* values, size_t beginPos, size_t size)
    {
        return doCreate(values, beginPos, size, true);
    }

    private DataArray doCreate(in float* values, size_t beginPos, size_t size, bool isConst)
    {
        assert(values);

        float[] a;
        a.length = size;
        a[0 .. size] = values[beginPos .. beginPos + size];

        return new NDataArray(a, isConst);
    }
}