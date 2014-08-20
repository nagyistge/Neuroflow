#pragma once

#include "nfdev.h"

namespace nf
{
    struct device_array_group
    {
        typedef idx_t key_t;

        device_array_group(const device_array_pool_ptr& pool);

        const device_array_ptr& add(const key_t& idx, idx_t size);
        const device_array_ptr& get(const key_t& idx) const;
        bool try_get(const key_t& idx, device_array_ptr& result) const;
        void zero();
        idx_t size() const;
        device_array_collection_t get_arrays() const;

    private:
        device_array_pool_ptr _pool;
        device_array_collection_t _arrays;
    };
}
