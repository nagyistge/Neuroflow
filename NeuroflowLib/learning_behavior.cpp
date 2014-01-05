#include "stdafx.h"
#include "learning_behavior.h"

USING;

bool learning_behavior::props_equals(const layer_behavior_ptr& other) const
{
    auto o = dynamic_pointer_cast<learning_behavior>(other);
    assert(o);
    return group_id == o->group_id;
}