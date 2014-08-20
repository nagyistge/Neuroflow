#pragma once

#include "../utils.h"

namespace nf
{
    struct cpp_utils : virtual utils
    {
        cpp_utils();

        void randomize_uniform(const device_array_ptr& deviceArray, float min, float max) override;
        void calculate_mse(supervised_batch& batch, const data_array_ptr& dataArray, idx_t valueIndex) override;
        void zero(const device_array_ptr& deviceArray) override;

    private:
        std::mt19937 generator;
    };
}
