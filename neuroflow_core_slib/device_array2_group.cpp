#include "stdafx.h"
#include "device_array2_group.h"
#include "device_array_management.h"
#include "device_array_pool.h"
#include "device_array2.h"

USING

device_array2_group::device_array2_group(const device_array_pool_ptr& pool) :
    _pool(pool)
{
    if (!_pool) throw_invalid_argument("Pool is null.");
}

const device_array2_ptr& device_array2_group::add(const key_t& idx, idx_t rowSize, idx_t colSize)
{
    _arrays.insert(make_pair(idx, _pool->create_array2(rowSize, colSize)));
    return _arrays.find(idx)->second;
}

const device_array2_ptr& device_array2_group::get(const key_t& idx) const
{
    auto it = _arrays.find(idx);
    if (it == _arrays.end()) throw_invalid_argument("Argument 'idx' is out of range.");
    return it->second;
}

bool device_array2_group::try_get(const key_t& idx, device_array2_ptr& result) const
{
    result = null;
    auto it = _arrays.find(idx);
    if (it != _arrays.end())
    {
        result = it->second;
        return true;
    }
    return false;
}

void device_array2_group::zero()
{
    if (_arrays.size() > 0 && _pool->is_allocated()) _pool->zero();
}

idx_t device_array2_group::size() const
{
    return from(_arrays) >> select([](const pair<key_t, device_array2_ptr*>& i) { return (*i.second)->size(); }) >> sum();
}

device_array2_collection_t device_array2_group::get_arrays() const
{
    return from(_arrays) >> select([](const pair<key_t, device_array2_ptr*>& i) { return (*i.second); }) >> to_vector();
}
