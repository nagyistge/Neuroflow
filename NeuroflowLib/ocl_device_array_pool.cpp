#include "stdafx.h"
#include "ocl_nf.h"

using namespace std;
using namespace nf;
using namespace cl;

ocl_device_array_pool::ocl_device_array_pool(const ocl_device_array_management_ptr& deviceArrayMan, const ocl_utils_ptr& utils);

bool ocl_device_array_pool::is_allocated() const override;
device_array_ptr ocl_device_array_pool::create_array(idx_t size) override;
device_array2_ptr ocl_device_array_pool::create_array2(idx_t rowSize, idx_t colSize) override;
void ocl_device_array_pool::allocate() override;
void ocl_device_array_pool::zero() override;

idx_t ocl_device_array_pool::reserve(idx_t size);
cl::Buffer ocl_device_array_pool::create_sub_buffer(unsigned beginOffset, unsigned size);