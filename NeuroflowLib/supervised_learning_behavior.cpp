#include "stdafx.h"
#include "supervised_learning_behavior.h"

USING

bool supervised_learning_behavior::props_equals(const layer_behavior_ptr& other) const
{
    if (!learning_behavior::props_equals(other)) return false;
    auto o = dynamic_cast<supervised_learning_behavior*>(other.get());
    assert(o);
    return weight_update_mode() == o->weight_update_mode() && optimization_type() == o->optimization_type();
}