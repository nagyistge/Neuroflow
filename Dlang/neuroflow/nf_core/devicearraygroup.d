import devicearraypool;
import devicearray;
import std.algorithm;

class DeviceArrayGroup
{
	this(DeviceArrayPool pool)
	{
		assert(pool);

		this._pool = pool;
	}

	DeviceArray add(in size_t idx, in size_t size)
	{
		if (idx >= _arrays.length) _arrays.length = idx + 1;
		return _arrays[idx] = _pool.createArray(size);
	}

	DeviceArray get(in size_t idx) 
	{
		return _arrays[idx];
	}

	bool tryGet(in size_t idx, out DeviceArray result)
	{
		if (idx < _arrays.length && _arrays[idx])
		{
			result = _arrays[idx];
			return true;
		}
		return false;
	}

	void zero()
	{
		if (_arrays.length > 0 && _pool.isAllocated()) _pool.zero();
	}

	@property size_t size() 
	{
		return _arrays.filter!(a => a).map!(a => a.size()).reduce!((a, b) => a + b);
	}

	@property auto arrays() 
	{
		return _arrays.filter!(a => a);
	}

	private DeviceArrayPool _pool;

	private DeviceArray[] _arrays;
}