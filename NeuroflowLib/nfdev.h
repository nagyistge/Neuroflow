#pragma once

#include "libs.h"
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
#define USING using namespace std; using namespace nf; using namespace linq;
#define _item_t(container) decltype(*std::begin(container))
#define _citem_t(container) decltype(*std::cbegin(container))