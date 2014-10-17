#pragma once

#include "nfdev.h"

namespace nf
{
    struct supervised_sample_entry
    {
        supervised_sample_entry() = delete;
        supervised_sample_entry(const data_array_ptr& input);
        supervised_sample_entry(const data_array_ptr& input, const data_array_ptr& desiredOutput, const data_array_ptr& actualOutput);

        const data_array_ptr& input() const;
        const data_array_ptr& desired_output() const;
        const data_array_ptr& actual_output() const;
        bool has_output() const;

    private:
        std::tuple<data_array_ptr, data_array_ptr, data_array_ptr> data;
    };
}