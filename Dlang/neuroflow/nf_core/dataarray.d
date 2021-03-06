﻿import devicearray;
import aliases;

interface DataArray : DeviceArray
{
	@property void* dataArrayPtr() nothrow;

	@property bool isConst() const nothrow;

	void read(size_t sourceBeginIndex, size_t count, float* targetPtr, size_t targetBeginIndex);

	void write(in float* sourceArray, in size_t sourceBeginIndex, size_t count, size_t targetBeginIndex);
}