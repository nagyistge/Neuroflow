import utils;
import std.algorithm;
import devicearray;
import ndevicearray;
import supervisedbatch;
import dataarray;

class NUtils : Utils
{
    void randomizeUniform(DeviceArray deviceArray, float min, float max)
    {
        assert(false, "TODO");
    }
    
    void calculateMSE(SupervisedBatch batch, DataArray dataArray, size_t valueIndex)
    {
        assert(false, "TODO");
    }
    
    void zero(DeviceArray deviceArray)
    {
        auto nda = cast(NDeviceArray)deviceArray;
        assert(nda);
        fill(nda.array, 0.0f);
    }
}