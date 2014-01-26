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
#include <algorithm>
#include <boost/algorithm/string.hpp>
#include <boost/mpl/assert.hpp>
#include <boost/filesystem.hpp>
#include <boost/optional.hpp>
#include <boost/property_tree/ptree.hpp>
#include <boost/lexical_cast.hpp>
#include <cpplinq.hpp>
#include "nf_object.h"
#include "typedefs.h"
#include "error.h"
#include "version.h"
#include "nf_helpers.h"
#include "get_vector_size.h"
#include "finally.h"
#include "props.h"

namespace linq = linqlike;

#define null nullptr
#define USING using namespace std; using namespace nf; using namespace cl; using namespace concurrency; using namespace linq;
#define _item_t(container) decltype(*std::begin(container))
#define _citem_t(container) decltype(*std::cbegin(container))