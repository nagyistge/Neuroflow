import nfcast;
import dataarray;
import ndataarray;
import devicearray;
import ndevicearray;
import devicearray2;
import ndevicearray2;

NDeviceArray toNative(DataArray array, bool allowNull = false)
{
    return fastCast!NDeviceArray(array, allowNull);
}

NDeviceArray toNative(DeviceArray array, bool allowNull = false)
{
    return fastCast!NDeviceArray(array, allowNull);
}

NDeviceArray2 toNative(DeviceArray2 array, bool allowNull = false)
{
    return fastCast!NDeviceArray2(array, allowNull);
}