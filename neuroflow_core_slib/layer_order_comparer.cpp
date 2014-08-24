#include "stdafx.h"
#include "layer_order_comparer.h"
#include "layer.h"
#include "layer_connections.h"

USING

bool layer_order_comparer::operator()(const layer_ptr& layer1, const layer_ptr& layer2)
{
    return compare(layer1, layer2) < 0 ? true : false;
}

int layer_order_comparer::compare(const layer_ptr& layer1, const layer_ptr& layer2)
{
    auto key = make_pair(layer1, layer2);
    
    auto result = results.find(key);
    if (result != results.end()) return result->second;

    auto rKey = make_pair(layer2, layer1);

    result = results.find(rKey);
    if (result != results.end()) return -result->second;

    int ret;
    if (is_below(layer1, layer2)) ret = -1;
    else if (is_below(layer2, layer1)) ret = 1;
    else ret = 0;

    results.insert(make_pair(key, ret));
    return ret;
}

bool layer_order_comparer::is_below(const layer_ptr& layer1, const layer_ptr& layer2)
{
    for (auto& output : layer1->output_connections().connected_layers(flow_direction::one_way | flow_direction::two_way))
    {
        if (output == layer2) return true;
    }
    for (auto& output : layer1->output_connections().connected_layers(flow_direction::one_way | flow_direction::two_way))
    {
        if (is_below(output, layer2)) return true;
    }
    return false;
}
