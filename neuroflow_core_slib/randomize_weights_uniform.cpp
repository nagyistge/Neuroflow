#include "stdafx.h"
#include "randomize_weights_uniform.h"

USING

bool randomize_weights_uniform::props_equals(const layer_behavior* other) const
{
    auto o = dynamic_cast<const randomize_weights_uniform*>(other);
    if (!o) return false;
    return _strength == o->_strength;
}

::size_t randomize_weights_uniform::get_hash_code() const
{
    return hash<float>()(_strength);
}