import devicearray;
import aliases;

interface DataArray : DeviceArray
{
	@property bool isConst() const;

	void read(in DoneFunc doneCallback, size_t sourceBeginIndex, size_t count, float* targetPtr, size_t targetBeginIndex);

	void write(in DoneFunc doneCallback, in float* sourceArray, in size_t sourceBeginIndex, size_t count, size_t targetBeginIndex);
}