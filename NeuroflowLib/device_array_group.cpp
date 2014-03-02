#include "stdafx.h"
#include "device_array_group.h"
#include "device_array_management.h"
#include "device_array_pool.h"
#include "device_array.h"

USING

device_array_group::device_array_group(const device_array_pool_ptr& pool) :
    _pool(pool)
{
    if (!_pool) throw_invalid_argument("Pool is null.");
}

const device_array_ptr& device_array_group::add(const key_t& idx, idx_t size)
{
    if (idx >= _arrays.size()) _arrays.resize(idx + 1);
    return _arrays[idx] = _pool->create_array(size);
}

const device_array_ptr& device_array_group::get(const key_t& idx) const
{
    return _arrays.at(idx);
}

bool device_array_group::try_get(const key_t& idx, device_array_ptr& result) const
{
    result = null;
    if (idx < _arrays.size() && _arrays[idx])
    {
        result = _arrays[idx];
        return true;
    }
    return false;
}

void device_array_group::zero()
{
    if (_arrays.size() > 0) _pool->zero();
}

idx_t device_array_group::size() const
{
    return from(_arrays) | where([](device_array_ptr& a) { return (bool)a; }) | select([](device_array_ptr& a) { return a->size(); }) | sum();
}

linq::enumerable<device_array_ptr> device_array_group::get_arrays() const
{
    return from(_arrays) | where([](device_array_ptr& a) { return (bool)a; });
}