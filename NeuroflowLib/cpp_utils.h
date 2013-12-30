#pragma once

#include "utils.h"

namespace nf
{
    struct cpp_utils : _implements utils
    {
        cpp_utils();

        void randomize_uniform(device_array_ptr& deviceArray, float min, float max) const override;
        void calculate_mse(const supervised_batch& batch, data_array_ptr& dataArray, idx_t valueIndex) const override;
        void zero(device_array_ptr& deviceArray) const override;

    private:
        std::mt19937 generator;
    };
}