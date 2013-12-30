#include "stdafx.h"
#include "cpp_data_array.h"

using namespace std;
using namespace nf;
using namespace concurrency;

cpp_data_array::cpp_data_array(float* internalArray, idx_t arraySize, bool isReadOnly) :
cpp_device_array(internalArray, arraySize),
isReadOnly(isReadOnly)
{
}

bool cpp_data_array::is_read_only() const
{
    return isReadOnly;
}

concurrency::task<void> cpp_data_array::read(idx_t sourceBeginIndex, idx_t count, float* targetPtr, idx_t targetBeginIndex) const
{
    verify_arg(sourceBeginIndex >= 0, "sourceBeginIndex");
    verify_arg(count > 0, "count");
    verify_arg(targetPtr != null, "targetPtr");
    verify_arg(targetBeginIndex >= 0, "targetBeginIndex");

    memcpy(targetPtr + targetBeginIndex, ptr() + sourceBeginIndex, count * sizeof(float));

    return create_do_nothing_task();
}

concurrency::task<void> cpp_data_array::write(float* sourceArray, idx_t sourceBeginIndex, idx_t count, idx_t targetBeginIndex)
{
    if (isReadOnly) throw_runtime_error("Device array is read only.");

    verify_arg(sourceBeginIndex >= 0, "sourceBeginIndex");
    verify_arg(count > 0, "count");
    verify_arg(sourceArray != null, "sourceArray");
    verify_arg(targetBeginIndex >= 0, "targetBeginIndex");

    memcpy(ptr() + targetBeginIndex, sourceArray + sourceBeginIndex, count * sizeof(float));

    return create_do_nothing_task();
}