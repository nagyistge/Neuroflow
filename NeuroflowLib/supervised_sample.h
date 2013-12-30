#pragma once

#include "nf.h"
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
        std::list<supervised_sample_entry>& entries();
        void add(const supervised_sample_entry& entry);
        void add(const data_array_ptr& input);
        void add(const data_array_ptr& input, const data_array_ptr& desiredOutput, const data_array_ptr& actualOutput);

    private:
        std::list<supervised_sample_entry> _entries;
    };
}