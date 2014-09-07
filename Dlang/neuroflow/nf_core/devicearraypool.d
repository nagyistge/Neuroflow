import devicearray;
import devicearray2; 

interface DeviceArrayPool
{
	@property bool isAllocated() const;

	DeviceArray createArray(size_t size);

	DeviceArray2 createArray2(size_t rowSize, size_t colSize);

	void allocate();

	void zero();
}