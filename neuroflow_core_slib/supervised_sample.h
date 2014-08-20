#pragma once

#include "nfdev.h"
#include "supervised_sample_entry.h"

namespace nf
{
    struct supervised_sample
    {
        supervised_sample() { }
        supervised_sample(const supervised_sample_entry& entry);
        supervised_sample(const data_array_ptr& input);
        supervised_sample(const data_array_ptr& input, const data_array_ptr& desiredOutput, const data_array_ptr& actualOutput);

        idx_t number_of_outputs() const;
        std::vector<supervised_sample_entry>& entries();
        void push_back(const supervised_sample_entry& entry);
        void push_back(const data_array_ptr& input);
        void push_back(const data_array_ptr& input, const data_array_ptr& desiredOutput, const data_array_ptr& actualOutput);

    private:
        std::vector<supervised_sample_entry> _entries;
    };
}