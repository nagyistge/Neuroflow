import devicearraypool;
import devicearray2;
import std.algorithm;
import aliases;

struct DeviceArray2Group
{
	this(DeviceArrayPool pool)
	{
		assert(pool);

		this._pool = pool;
	}

	DeviceArray2 add(in RowCol idx, size_t rowSize, size_t colSize)
	{
		auto a = _pool.createArray2(rowSize, colSize);
		_arrays[idx] = a;
		return a;
	}

	DeviceArray2 get(in RowCol idx)
	{
		auto a = _arrays.get(idx, null);
		assert(a);
		return a;
	}

	bool tryGet(in RowCol idx, out DeviceArray2 result)
	{
		return (result = _arrays.get(idx, null)) !is null;
	}

	void zero()
	{
		if (_arrays.length > 0 && _pool.isAllocated()) _pool.zero();
	}

	@property size_t size()
	{
		return _arrays.byValue.filter!(a => a).map!(a => a.size()).reduce!((a, b) => a + b);
	}

	@property auto arrays()
	{
		return _arrays.byValue.filter!(a => a);
	}

    private DeviceArrayPool _pool;

	private DeviceArray2[RowCol] _arrays;
}
