import devicearray;

interface DeviceArray2 : DeviceArray
{
    @property size_t size1();

    @property size_t size2();

	@property void* deviceArray2Ptr();
}
