#include "stdafx.h"
#include "layer_behavior.h"

USING;

bool layer_behavior::equals(const layer_behavior_ptr& other) const
{
    if (!other) return false;
    if (this == other.get()) return true;
    if (typeid(this) == typeid(other)) return props_equals(other);
    return false;
}