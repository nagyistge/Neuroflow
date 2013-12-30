#pragma once

#include <memory>
#include <vector>
#include <sstream>
#include <exception>
#include <string>
#include <tuple>
#include <functional>
#include <list>
#include <unordered_set>
#include <unordered_map>
#include <boost/algorithm/string/replace.hpp>
#include <boost/mpl/assert.hpp>
#include <boost/algorithm/string/find.hpp>
#include <boost/filesystem.hpp>
#include <boost/optional.hpp>
#include "nf_object.h"
#include "error.h"

#define null nullptr
#define _interface struct
#define _implements virtual

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