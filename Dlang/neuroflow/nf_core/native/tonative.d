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

float[] toArray(DeviceArray array, bool nullable = false)
{
	auto a = toNativeDeviceArray(array, nullable);
	return a is null ? null : a.array;
}

NDeviceArray toNativeDeviceArray(DeviceArray2 array, bool nullable = false)
{
	return toNative!(NDeviceArray, "deviceArrayPtr")(array, nullable);
}

float[] toArray(DeviceArray2 array, bool nullable = false)
{
	auto a = toNativeDeviceArray(array, nullable);
	return a is null ? null : a.array;
}

NDeviceArray2 toNativeDeviceArray2(DeviceArray2 array, bool nullable = false)
{
	return toNative!(NDeviceArray2, "deviceArray2Ptr")(array, nullable);
}

NDeviceArray toNativeDeviceArray(DataArray array, bool nullable = false)
{
	return toNative!(NDeviceArray, "deviceArrayPtr")(array, nullable);
}

float[] toArray(DataArray array, bool nullable = false)
{
	auto a = toNativeDeviceArray(array, nullable);
	return a is null ? null : a.array;
}

T toNative(T, string propName)(DeviceArray array, bool nullable)
{
	if (!nullable) enforce(array, eNull); else if (array is null) return null;
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