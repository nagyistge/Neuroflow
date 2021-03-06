#pragma once

#include "nfdev.h"

namespace nf
{
    struct device_array2_group
    {
        typedef std::pair<idx_t, idx_t> key_t;

        device_array2_group(const device_array_pool_ptr& pool);

        const device_array2_ptr& add(const key_t& idx, idx_t rowSize, idx_t colSize);
        const device_array2_ptr& get(const key_t& idx) const;
        bool try_get(const key_t& idx, device_array2_ptr& result) const;
        void zero();
        idx_t size() const;
        linq::enumerable<device_array2_ptr> get_arrays() const;

    private:
        device_array_pool_ptr _pool;
        std::map<key_t, device_array2_ptr> _arrays;
    };
}