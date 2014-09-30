import devicearray;

interface DeviceArray2 : DeviceArray
{
    @property size_t size1() nothrow;

    @property size_t size2() nothrow;

	@property void* deviceArray2Ptr() nothrow;
}
