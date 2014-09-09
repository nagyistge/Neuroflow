import utils;
import std.algorithm;
import devicearray;
import ndevicearray;
import supervisedbatch;
import dataarray;
import ncast;
import std.random;
import std.exception;

class NUtils : Utils
{
    void randomizeUniform(DeviceArray deviceArray, float min, float max)
    {
        auto nArray = toNative(deviceArray);
        foreach (ref v; nArray.array)
        {
            v = uniform(min, max);
        }
    }
    
    void calculateMSE(SupervisedBatch batch, DataArray dataArray, size_t valueIndex)
    {
        auto nArray = toNative(dataArray);
        assert(nArray);
        enforce(valueIndex >= 0 && valueIndex < dataArray.size, "Value index is invalid.");

        auto arr = nArray.array;
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
                    auto actualOutput = toNative(entry.actualOutput);
                    auto desiredOutput = toNative(entry.desiredOutput);
                    assert(actualOutput);
                    assert(desiredOutput);

                    float cMse = 0.0f;
                    
                    size_t size = actualOutput.size;
                    auto doArr = desiredOutput.array;
                    auto aoArr = actualOutput.array;
                    assert(doArr && doArr.length);
                    assert(aoArr && aoArr.length);
                    assert(aoArr.length == doArr.length);

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