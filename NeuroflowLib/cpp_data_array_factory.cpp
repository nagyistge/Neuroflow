#include "stdafx.h"
#include "cpp_data_array_factory.h"
#include "cpp_data_array.h"

USING;

data_array_ptr cpp_data_array_factory::create(idx_t size, float fill)
{
    verify_arg(size > 0, "size");

    float* data = new float[size];
    try
    {
        for (idx_t i = 0; i < size; i++) data[i] = fill;
        return make_shared<cpp_data_array>(data, size, false);
    }
    catch (...)
    {
        delete[] data;
        throw;
    }
}

data_array_ptr cpp_data_array_factory::create(float* values, idx_t beginPos, idx_t size)
{
    return create(values, beginPos, size, false);
}

data_array_ptr cpp_data_array_factory::create_const(float* values, idx_t beginPos, idx_t size)
{
    return create(values, beginPos, size, true);
}

data_array_ptr cpp_data_array_factory::create(float* values, idx_t beginPos, idx_t size, bool isConst)
{
    verify_arg(values != null, "values");
    verify_arg(beginPos >= 0, "beginPos");
    verify_arg(size > 0, "size");

    float* data = new float[size];
    try
    {
        memcpy(data, values + beginPos, size * sizeof(float));
        return make_shared<cpp_data_array>(data, size, isConst);
    }
    catch (...)
    {
        delete[] data;
        throw;
    }
}