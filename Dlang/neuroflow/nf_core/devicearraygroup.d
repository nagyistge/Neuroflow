import devicearraypool;
import devicearray;

class DeviceArrayGroup
{
	this(DeviceArrayPool pool)
	{
		this._pool = pool;
	}

	DeviceArray add(in size_t idx, in size_t size)
	{
		if (idx >= _arrays.length) _arrays.length = idx + 1;
		return _arrays[idx] = _pool.createArray(size);
	}

	DeviceArray get(in size_t idx) const
	{
		return arrays[idx];
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

	size_t size() const @property
	{
		/*
		return from(_arrays) >> where([](const device_array_ptr& a) { return (bool)a; }) >> select([](const device_array_ptr& a) { return a->size(); }) >> sum();
		*/
	}

	DeviceArray[] arrays() const @property
	{
		/*
		return from(_arrays) >> where([](const device_array_ptr& a) { return (bool)a; }) >> to_vector();
		*/
	}

	private DeviceArrayPool _pool;
	private DeviceArray[] _arrays;
}