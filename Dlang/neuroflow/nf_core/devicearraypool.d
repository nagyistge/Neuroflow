import devicearray;
import devicearray2; 

interface DeviceArrayPool
{
	bool isAllocated() const;

	DeviceArray createArray(in size_t size);

	DeviceArray2 createArray2(in size_t rowSize, in size_t colSize);

	void allocate();

	void zero();
}
