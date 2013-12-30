#include "stdafx.h"
#include "supervised_batch.h"

using namespace std;
using namespace nf;

supervised_batch::supervised_batch(const supervised_sample& sample)
{
    add(sample);
}

supervised_batch::supervised_batch(const supervised_sample_entry& entry)
{
    add(entry);
}

supervised_batch::supervised_batch(const data_array_ptr& input)
{
    add(input);
}

supervised_batch::supervised_batch(const data_array_ptr& input, const data_array_ptr& desiredOutput, const data_array_ptr& actualOutput)
{
    add(input, desiredOutput, actualOutput);
}

std::list<supervised_sample> supervised_batch::samples()
{
    return _samples;
}

void supervised_batch::add(const supervised_sample& sample)
{
    _samples.emplace_back(sample);
}

void supervised_batch::add(const supervised_sample_entry& entry)
{
    _samples.emplace_back(entry);
}

void supervised_batch::add(const data_array_ptr& input)
{
    _samples.emplace_back(input);
}

void supervised_batch::add(const data_array_ptr& input, const data_array_ptr& desiredOutput, const data_array_ptr& actualOutput) 
{
    _samples.emplace_back(input, desiredOutput, actualOutput);
}