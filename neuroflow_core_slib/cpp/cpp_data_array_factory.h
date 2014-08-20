#pragma once

#include "cpp_nfdev.h"
#include "../data_array_factory.h"

namespace nf
{
    struct cpp_data_array_factory : virtual data_array_factory
    {
        data_array_ptr create(idx_t size, float fill) override;
        data_array_ptr create(float* values, idx_t beginPos, idx_t size) override;
        data_array_ptr create_const(float* values, idx_t beginPos, idx_t size) override;

    private:
        data_array_ptr create(float* values, idx_t beginPos, idx_t size, bool isConst);
    };
}
