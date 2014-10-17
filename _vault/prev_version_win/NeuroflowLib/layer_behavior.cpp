#include "stdafx.h"
#include "layer_behavior.h"

USING

bool layer_behavior::equals(const equatable& other) const
{
    if (this->type_name() == other.type_name()) return props_equals(dynamic_cast<const layer_behavior*>(&other));
    return false;
}

bool layer_behavior::props_equals(const layer_behavior* other) const
{
    return other != null;
}