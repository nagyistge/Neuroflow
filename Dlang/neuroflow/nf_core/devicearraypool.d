import devicearray;
import devicearray2; 
import computationcontextfactory;
import nfdefs;

interface DeviceArrayPool
{
	@property bool isAllocated() const;

	DeviceArray createArray(size_t size);

	DeviceArray2 createArray2(size_t rowSize, size_t colSize);

	void allocate();

	void zero();
}

unittest
{
    auto ctx = ComputationContextFactory.instance.createContext(NativeContext);
    assert(ctx);
}