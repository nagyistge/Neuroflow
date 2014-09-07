import devicearray;
import supervisedbatch;
import dataarray;

interface Utils
{
    void randomizeUniform(DeviceArray deviceArray, float min, float max);

    void calculateMSE(SupervisedBatch batch, DataArray dataArray, size_t valueIndex);

    void zero(DeviceArray deviceArray);
}