#include "stdafx.h"
#include "cpp_utils.h"
#include "cpp_conv.h"
#include "cpp_device_array.h"

using namespace std;
using namespace nf;

void cpp_utils::zero(device_array_ptr& deviceArray) const
{
    auto& cppArray = to_cpp(deviceArray, false);
    memset(cppArray->ptr(), 0, sizeof(float)* cppArray->size());
}