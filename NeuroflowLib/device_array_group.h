#pragma once

#include "nfdev.h"

namespace nf
{
    struct device_array_group
    {
        typedef idx_t key_t;

        device_array_group(const device_array_pool_ptr& pool);

        const device_array_ptr& add(key_t idx, idx_t size);
        const device_array_ptr& get(key_t idx) const;
        void zero();
        idx_t size() const;
        linq::enumerable<device_array*> get_arrays() const;

    private:
        device_array_pool_ptr _pool;
        std::map<key_t, device_array_ptr> _arrays;
    };
}