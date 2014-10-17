#pragma once

#include "nfdev.h"
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

        std::vector<supervised_sample>& samples();
        std::vector<supervised_sample>::reference back();
        std::vector<supervised_sample>::reference new_back();
        void push_back(const supervised_sample& sample);
        void push_back(const supervised_sample_entry& entry);
        void push_back(const data_array_ptr& input);
        void push_back(const data_array_ptr& input, const data_array_ptr& desiredOutput, const data_array_ptr& actualOutput);

    private:
        std::vector<supervised_sample> _samples;
    };
}