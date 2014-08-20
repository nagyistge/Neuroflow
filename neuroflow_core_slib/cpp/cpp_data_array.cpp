#include "../stdafx.h"
#include "cpp_data_array.h"

USING

cpp_data_array::cpp_data_array(float* internalArray, idx_t arraySize, bool isConst) :
cpp_device_array(internalArray, arraySize),
isConst(isConst)
{
}

bool cpp_data_array::is_const() const
{
    return isConst;
}

boost::shared_future<void> cpp_data_array::read(idx_t sourceBeginIndex, idx_t count, float* targetPtr, idx_t targetBeginIndex) 
{
    verify_arg(sourceBeginIndex >= 0, "sourceBeginIndex");
    verify_arg(count > 0, "count");
    verify_arg(targetPtr != null, "targetPtr");
    verify_arg(targetBeginIndex >= 0, "targetBeginIndex");

    verify_if_accessible();

    memcpy(targetPtr + targetBeginIndex, ptr() + sourceBeginIndex, count * sizeof(float));

    return create_do_nothing_task();
}

boost::shared_future<void> cpp_data_array::write(float* sourceArray, idx_t sourceBeginIndex, idx_t count, idx_t targetBeginIndex)
{
    verify_arg(sourceBeginIndex >= 0, "sourceBeginIndex");
    verify_arg(count > 0, "count");
    verify_arg(sourceArray != null, "sourceArray");
    verify_arg(targetBeginIndex >= 0, "targetBeginIndex");

    verify_if_accessible();

    memcpy(ptr() + targetBeginIndex, sourceArray + sourceBeginIndex, count * sizeof(float));

    return create_do_nothing_task();
}

void cpp_data_array::verify_if_accessible() const
{
    if (isConst) throw_runtime_error("Const data array is not accessible.");
}
