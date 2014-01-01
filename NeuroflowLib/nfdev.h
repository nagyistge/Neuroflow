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
#include "nf_object.h"
#include "typedefs.h"
#include "error.h"
#include "version.h"
#include "nf_helpers.h"
#include "get_vector_size.h"

#define null nullptr
#define USING using namespace std; using namespace nf; using namespace cl; using namespace concurrency;