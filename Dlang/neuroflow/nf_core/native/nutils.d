import utils;
import std.algorithm;
import devicearray;
import ndevicearray;
import supervisedbatch;
import dataarray;
import std.random;
import std.exception;
import tonative;

class NUtils : Utils
{
    void randomizeUniform(DeviceArray deviceArray, float min, float max)
    {
        foreach (ref v; toArray(deviceArray))
        {
            v = uniform(min, max);
        }
    }
    
    void calculateMSE(SupervisedBatch batch, DataArray dataArray, size_t valueIndex)
    {
        auto arr = toArray(dataArray);
        assert(arr);
        enforce(valueIndex >= 0 && valueIndex < dataArray.size, "Value index is invalid.");
        assert(arr && arr.length);
        arr[valueIndex] = 0.0f;
        float count = 0.0f;
        assert(batch.samples !is null);
        foreach (sample; batch.samples)
        {
            assert(sample.entries !is null);
            foreach (entry; sample.entries)
            {
                if (entry.hasOutput)
                {
                    float cMse = 0.0f;
                    
                    auto doArr = toArray(entry.desiredOutputs);
                    auto aoArr = toArray(entry.actualOutputs);
                    assert(doArr && doArr.length);
                    assert(aoArr && aoArr.length);
                    assert(aoArr.length == doArr.length);

					size_t size = aoArr.length;
					for (size_t x = 0; x < size; x++)
                    {
                        float error = (doArr[x] - aoArr[x]) * 0.5f;
                        cMse += error * error;
                    }
                    arr[valueIndex] += cMse / cast(float)size;
                    
                    count++;
                }
            }
        }
        
        if (count != 0.0f) arr[valueIndex] /= count;
    }
    
    void zero(DeviceArray deviceArray)
    {
        auto nda = cast(NDeviceArray)deviceArray;
        assert(nda);
        fill(nda.array, 0.0f);
    }
}