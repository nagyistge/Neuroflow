import devicearray;
import devicearray2; 
import devicearraypool;

interface DeviceArrayManagement
{
	DeviceArray createArray(bool copyOptimized, size_t size);

	DeviceArray2 createArray2(bool copyOptimized, size_t rowSize, size_t colSize);

	void copy(DeviceArray from, size_t fromIndex, DeviceArray to, size_t toIndex, size_t size);

	DeviceArrayPool createPool(bool copyOptimized);
}
