#pragma once

#include "ocl_nfdev.h"
#include "ocl_contexted.h"

namespace nf
{
    struct ocl_1d_range
    {
        ocl_1d_range() = delete;
        ocl_1d_range(idx_t workItemsCount, idx_t localSize) : sizes(workItemsCount, localSize) { }

        idx_t work_items_count() const 
        {
            return sizes.first;
        }

        idx_t local_size() const
        {
            return sizes.second;
        }

    private:
        std::pair<idx_t, idx_t> sizes;
    };

    struct ocl_sizes : ocl_contexted
    {
        ocl_sizes(const ocl_computation_context_wptr& context);

        idx_t get_optimal_global_size(idx_t workItemsCount, idx_t vectorSize) const;
        ocl_1d_range get_io_reduce_sizes_input(idx_t inputSize, idx_t vectorSize, idx_t outputSize) const;
        ocl_1d_range get_io_reduce_sizes_output(idx_t inputSize, idx_t outputSize, idx_t vectorSize) const;

    private:
        inline idx_t get_best_local_size(idx_t size) const;
        idx_t to_power_of_two(idx_t value) const;
        inline bool is_power_of_two(idx_t value) const;
    };
}