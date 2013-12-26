#pragma once

#include <memory>
#include <vector>
#include <sstream>
#include <exception>
#include "nf_object.h"
#include "error.h"

#define null nullptr

namespace nf
{
    typedef ::size_t idx_t;

    typedef std::shared_ptr<nf_object> nf_object_ptr;

    struct device_array;
    typedef std::shared_ptr<device_array> device_array_ptr;

    struct device_array2;
    typedef std::shared_ptr<device_array2> device_array2_ptr;

    struct device_array_pool;
    typedef std::shared_ptr<device_array_pool> device_array_pool_ptr;
}