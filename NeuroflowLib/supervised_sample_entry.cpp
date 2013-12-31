#include "stdafx.h"
#include "supervised_sample_entry.h"
#include "data_array.h"

USING;

supervised_sample_entry::supervised_sample_entry(const data_array_ptr& input) : data(input, null, null) 
{ 
    verify_arg(input != null, "input");
}

supervised_sample_entry::supervised_sample_entry(const data_array_ptr& input, const data_array_ptr& desiredOutput, const data_array_ptr& actualOutput) :
data(input, desiredOutput, actualOutput)
{
    verify_arg(input != null, "input");
    verify_arg(desiredOutput != null, "desiredOutput");
    verify_arg(actualOutput != null, "actualOutput");
    verify_arg(desiredOutput->size() == actualOutput->size(), "desiredOutput, actualOutput");
}

const data_array_ptr& supervised_sample_entry::input() const
{
    return get<0>(data);
}

const data_array_ptr& supervised_sample_entry::desired_output() const
{
    return get<1>(data);
}

const data_array_ptr& supervised_sample_entry::actual_output() const
{
    return get<2>(data);
}

bool supervised_sample_entry::has_output() const
{
    return desired_output() != null && actual_output() != null;
}