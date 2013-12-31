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
#include <ppltasks.h>
#include <random>
#include <boost/algorithm/string/replace.hpp>
#include <boost/mpl/assert.hpp>
#include <boost/algorithm/string/find.hpp>
#include <boost/filesystem.hpp>
#include <boost/optional.hpp>
#include <boost/property_tree/ptree.hpp>
#include "nf_object.h"
#include "error.h"
#include "version.h"
#include "nf_helpers.h"

#define null nullptr
#define USING using namespace std; using namespace nf; using namespace cl; using namespace concurrency

namespace nf
{
    const wchar_t* cpp_context = L"cpp";
    const wchar_t* ocl_context = L"ocl";

    typedef ::size_t idx_t;

    typedef std::shared_ptr<nf_object> nf_object_ptr;

    struct computation_context;
    typedef std::shared_ptr<computation_context> computation_context_ptr;

    struct cc_factory_adapter;
    typedef std::shared_ptr<cc_factory_adapter> cc_factory_adapter_ptr;

    struct device_array;
    typedef std::shared_ptr<device_array> device_array_ptr;

    struct device_array2;
    typedef std::shared_ptr<device_array2> device_array2_ptr;

    struct device_array_pool;
    typedef std::shared_ptr<device_array_pool> device_array_pool_ptr;

    struct device_array_management;
    typedef std::shared_ptr<device_array_management> device_array_management_ptr;

    struct data_array;
    typedef std::shared_ptr<data_array> data_array_ptr;

    struct data_array_factory;
    typedef std::shared_ptr<data_array_factory> data_array_factory_ptr;

    struct utils;
    typedef std::shared_ptr<utils> utils_ptr;

    struct supervised_batch;
    struct supervised_sample;
    struct supervised_sample_entry;
}