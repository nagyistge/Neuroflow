import devicearray;

interface DataArray : DeviceArray
{
	bool is_const() const;

	void read(size_t sourceBeginIndex, size_t count, float* targetPtr, size_t targetBeginIndex);

	void write(float* sourceArray, size_t sourceBeginIndex, size_t count, size_t targetBeginIndex);
}