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

const device_array_ptr& device_array_group::add(key_t idx, idx_t size)
{
    _arrays.insert(make_pair(idx, _pool->create_array(size)));
    return _arrays.find(idx)->second;
}

const device_array_ptr& device_array_group::get(key_t idx) const
{
    auto it = _arrays.find(idx);
    if (it == _arrays.end()) throw_invalid_argument("Argument 'idx' is out of range.");
    return it->second;
}

void device_array_group::zero()
{
    _pool->zero();
}