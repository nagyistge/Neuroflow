#include "stdafx.h"
#include "supervised_sample.h"

USING

supervised_sample::supervised_sample(const supervised_sample_entry& entry)
{
    push_back(entry);
}

supervised_sample::supervised_sample(const data_array_ptr& input)
{
    push_back(input);
}

supervised_sample::supervised_sample(const data_array_ptr& input, const data_array_ptr& desiredOutput, const data_array_ptr& actualOutput)
{
    push_back(input, desiredOutput, actualOutput);
}

idx_t supervised_sample::number_of_outputs() const
{
    idx_t c = 0;
    for (auto& e : _entries) if (e.has_output()) c++;
    return c;
}

std::vector<supervised_sample_entry>& supervised_sample::entries()
{
    return _entries;
}

void supervised_sample::push_back(const supervised_sample_entry& entry)
{
    _entries.emplace_back(entry);
}

void supervised_sample::push_back(const data_array_ptr& input)
{
    _entries.emplace_back(input);
}

void supervised_sample::push_back(const data_array_ptr& input, const data_array_ptr& desiredOutput, const data_array_ptr& actualOutput)
{
    _entries.emplace_back(input, desiredOutput, actualOutput);
}
