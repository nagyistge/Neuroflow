#pragma once

#include "nf.h"
#include "supervised_sample.h"

namespace nf
{
    struct supervised_batch
    {
        supervised_batch() { }
        supervised_batch(const supervised_sample& sample);
        supervised_batch(const supervised_sample_entry& entry);
        supervised_batch(const data_array_ptr& input);
        supervised_batch(const data_array_ptr& input, const data_array_ptr& desiredOutput, const data_array_ptr& actualOutput);

        std::list<supervised_sample> samples();
        void add(const supervised_sample& sample);
        void add(const supervised_sample_entry& entry);
        void add(const data_array_ptr& input);
        void add(const data_array_ptr& input, const data_array_ptr& desiredOutput, const data_array_ptr& actualOutput);

    private:
        std::list<supervised_sample> _samples;
    };
}