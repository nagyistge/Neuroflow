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
    _arrays.insert(make_pair(idx, _pool->create_array(size)));
    return _arrays.find(idx)->second;
}

const device_array_ptr& device_array_group::get(const key_t& idx) const
{
    auto it = _arrays.find(idx);
    if (it == _arrays.end()) throw_invalid_argument("Argument 'idx' is out of range.");
    return it->second;
}

void device_array_group::zero()
{
    _pool->zero();
}

idx_t device_array_group::size() const
{
    return from(_arrays) | select([](pair<key_t, device_array_ptr*>& i) { return (*i.second)->size(); }) | sum();
}

linq::enumerable<device_array_ptr> device_array_group::get_arrays() const
{
    return from(_arrays) | select([](pair<key_t, device_array_ptr*>& i) { return (*i.second); });
}