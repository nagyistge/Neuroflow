#include "stdafx.h"
#include "randomize_weights_uniform.h"

USING

bool randomize_weights_uniform::props_equals(const layer_behavior* other) const
{
    if (!learning_init_behavior::props_equals(other)) return false;
    auto o = dynamic_cast<const randomize_weights_uniform*>(other);
    if (!o) return false;
    return _strength == o->_strength;
}