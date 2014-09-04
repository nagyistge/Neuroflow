import devicearray;
import aliases;

interface DataArray : DeviceArray
{
	bool isConst() const;

	void read(in DoneFunc doneCallback, in size_t sourceBeginIndex, in size_t count, in float* targetPtr, in size_t targetBeginIndex);

	void write(in DoneFunc doneCallback, in float* sourceArray, in size_t sourceBeginIndex, in size_t count, in size_t targetBeginIndex);
}