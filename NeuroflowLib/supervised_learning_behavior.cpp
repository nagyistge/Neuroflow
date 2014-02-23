#include "stdafx.h"
#include "supervised_learning_behavior.h"

USING

bool supervised_learning_behavior::props_equals(const layer_behavior* other) const
{
    if (!learning_behavior::props_equals(other)) return false;
    auto o = dynamic_cast<const supervised_learning_behavior*>(other);
    if (!o) return false;
    return weight_update_mode() == o->weight_update_mode() && optimization_type() == o->optimization_type();
}