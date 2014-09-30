import devicearray;
import devicearray2;
import dataarray;
import ndevicearray;
import ndevicearray2;
import ndataarray;
import std.exception;

string eNull = "Array argument is null.";
string eInvalid = "Array argument type is incompatible.";

NDeviceArray toNativeDeviceArray(DeviceArray array, bool nullable = false)
{
	return toNative!(NDeviceArray, "deviceArrayPtr")(array, nullable);
}

NDeviceArray toNativeDeviceArray(DeviceArray2 array, bool nullable = false)
{
	return toNative!(NDeviceArray, "deviceArrayPtr")(array, nullable);
}

NDeviceArray toNativeDeviceArray(DataArray array, bool nullable = false)
{
	return toNative!(NDeviceArray, "deviceArrayPtr")(array, nullable);
}

NDeviceArray toNative(T, string propName)(DeviceArray array, bool nullable)
{
	if (!nullable) enforce(array, eNull);
	debug
	{
		auto a = cast(T)array;
		if (a is null) enforce(array, eInvalid);
		return a;
	}
	else
	{
		mixin("return cast(T)(array." ~ propName ~ ");");
	}
}