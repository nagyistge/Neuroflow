#include "stdafx.h"
#include "mlp_forward_node.h"
#include "device_array.h"

USING

idx_t mlp_forward_node::size() const
{
    return bias->size();
}