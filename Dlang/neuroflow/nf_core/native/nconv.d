import dataarray;
import ndataarray;
import devicearray;
import ndevicearray;
import devicearray2;
import ndevicearray2;
import std.exception;

NDataArray toNative(DataArray array, bool nullable = false)
{
    auto r = cast(NDataArray)array;
    void* p = cast(void*)r;
    if (!nullable) enforce(r, "Array is null.");
    return r;
}

NDeviceArray toNative(DeviceArray array, bool nullable = false)
{
    auto r = cast(NDeviceArray)array;
    if (!nullable) enforce(r, "Array is null.");
    return r;
}

NDeviceArray2 toNative(DeviceArray2 array, bool nullable = false)
{
    auto r = cast(NDeviceArray2)array;
    if (!nullable) enforce(r, "Array is null.");
    return r;
}