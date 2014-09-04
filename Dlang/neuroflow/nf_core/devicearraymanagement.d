import devicearray;
import devicearray2; 
import devicearraypool;

interface DeviceArrayManagement
{
	DeviceArray createArray(in bool copyOptimized, in size_t size);

	DeviceArray2 createArray2(in bool copyOptimized, in size_t rowSize, in size_t colSize);

	void copy(DeviceArray from, in size_t fromIndex, DeviceArray to, in size_t toIndex, in size_t size);

	DeviceArrayPool createPool(in bool copyOptimized);
}
