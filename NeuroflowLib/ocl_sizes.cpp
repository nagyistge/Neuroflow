#include "stdafx.h"
#include "ocl_sizes.h"
#include "ocl_computation_context.h"

USING;

ocl_sizes::ocl_sizes(const ocl_computation_context_wptr& context) :
ocl_contexted(context)
{
}

idx_t ocl_sizes::get_optimal_global_size(idx_t workItemsCount, idx_t vectorSize) const
{
    auto ctx = lock_context();

    idx_t gs = workItemsCount / vectorSize;
    idx_t mws = ctx->max_work_group_size();
    if (gs > mws)
    {
        gs -= gs % mws;
    }
    else if (gs > ctx->preferred_workgroup_size_mul())
    {
        gs -= gs % ctx->preferred_workgroup_size_mul();
    }
    return to_power_of_two(gs);
}

ocl_1d_range ocl_sizes::get_io_reduce_sizes_input(idx_t inputSize, idx_t vectorSize, idx_t outputSize) const
{
    idx_t localSize = get_best_local_size(inputSize / vectorSize);
    return ocl_1d_range(outputSize * localSize, localSize);
}

ocl_1d_range ocl_sizes::get_io_reduce_sizes_output(idx_t inputSize, idx_t outputSize, idx_t vectorSize) const
{
    idx_t localSize = get_best_local_size(inputSize / vectorSize);
    return ocl_1d_range(outputSize * localSize, localSize);
}

idx_t ocl_sizes::get_best_local_size(idx_t size) const
{
    auto ctx = lock_context();

    idx_t pwsmul = ctx->preferred_workgroup_size_mul();
    if (size < pwsmul) return to_power_of_two(size);
    idx_t rem = size % pwsmul;
    size -= rem;
    idx_t maxws = ctx->max_work_item_sizes()[0];
    if (size > maxws) return maxws;
    return to_power_of_two(size);
}

idx_t ocl_sizes::to_power_of_two(idx_t value) const
{
    // TODO: Optimize this!
    while (!is_power_of_two(value)) value--;
    return value;
}

bool ocl_sizes::is_power_of_two(idx_t value) const
{
    return (value != 0) && ((value & (value - 1)) == 0);
}