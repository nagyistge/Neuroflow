interface DeviceArray
{
    @property size_t size() nothrow;

	@property void* deviceArrayPtr() nothrow;
}