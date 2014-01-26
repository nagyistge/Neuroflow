#include "stdafx.h"
#include "supervised_batch.h"

USING

supervised_batch::supervised_batch(const supervised_sample& sample)
{
    push_back(sample);
}

supervised_batch::supervised_batch(const supervised_sample_entry& entry)
{
    push_back(entry);
}

supervised_batch::supervised_batch(const data_array_ptr& input)
{
    push_back(input);
}

supervised_batch::supervised_batch(const data_array_ptr& input, const data_array_ptr& desiredOutput, const data_array_ptr& actualOutput)
{
    push_back(input, desiredOutput, actualOutput);
}

std::list<supervised_sample> supervised_batch::samples()
{
    return _samples;
}

std::list<supervised_sample>::reference supervised_batch::back()
{
    return _samples.back();
}

std::list<supervised_sample>::reference supervised_batch::new_back()
{
    _samples.emplace_back();
    return back();
}

void supervised_batch::push_back(const supervised_sample& sample)
{
    _samples.emplace_back(sample);
}

void supervised_batch::push_back(const supervised_sample_entry& entry)
{
    _samples.emplace_back(entry);
}

void supervised_batch::push_back(const data_array_ptr& input)
{
    _samples.emplace_back(input);
}

void supervised_batch::push_back(const data_array_ptr& input, const data_array_ptr& desiredOutput, const data_array_ptr& actualOutput)
{
    _samples.emplace_back(input, desiredOutput, actualOutput);
}