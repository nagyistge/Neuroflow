#include "stdafx.h"
#include "mlp_backward_node.h"
#include "device_array.h"

USING

idx_t mlp_backward_node::size() const
{
    if (has_gradients()) return bias_gradients->size();
    if (has_gradient_sums()) return bias_gradient_sums->size();
}

bool mlp_backward_node::has_gradients() const
{
    return bias_gradients != null && gradients.size() > 0;
}

bool mlp_backward_node::has_gradient_sums() const
{
    return bias_gradient_sums != null && gradient_sums.size() > 0;
}

bool mlp_backward_node::is_last() const
{
    return (bool)net_outputs;
}