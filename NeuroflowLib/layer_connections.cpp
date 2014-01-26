#include "stdafx.h"
#include "layer_connections.h"
#include "layer.h"

USING

layer_connections::layer_connections(layer_connections::connection_type type) :
_type(type)
{
}

bool layer_connections::must_init() const
{
    return !_connectedLayer;
}

void layer_connections::init(const layer_ptr& connectedLayer)
{
    assert(!_connectedLayer);
    assert(connectedLayer);
    _connectedLayer = connectedLayer;
}

void layer_connections::add_one_way(const layer_ptr& layer)
{
    add(layer, flow_direction::one_way);
}

void layer_connections::add_two_way(const layer_ptr& layer)
{
    add(layer, flow_direction::two_way);
}

void layer_connections::add_one_way_to_source(const layer_ptr& layer)
{
    add(layer, flow_direction::one_way_to_source);
}

void layer_connections::add(const layer_ptr& layer, flow_direction direction)
{
    verify_arg(layer != null, "layer");
    verify_arg(direction == flow_direction::one_way || direction == flow_direction::one_way_to_source || direction == flow_direction::two_way, "direction");

    if (find_if(_otherLayers.cbegin(), _otherLayers.cend(), [&](_citem_t(_otherLayers) p) { return p.second == layer; }) == _otherLayers.cend())
    {
        with_other_side_update_suppressed(layer,
        [&]()
        {
            if (_type == connection_type::output)
            {
                layer->input_connections().add(_connectedLayer, direction);
            }
            else
            {
                layer->output_connections().add(_connectedLayer, direction);
            }
        });
        _otherLayers.emplace_back(direction, layer);
    }
}

bool layer_connections::remove(const layer_ptr& layer)
{
    verify_arg(layer != null, "item");

    auto toRemove = find_if(_otherLayers.cbegin(), _otherLayers.cend(), [&](_citem_t(_otherLayers) p) { return p.second == layer; });
    if (toRemove != _otherLayers.cend())
    {
        _otherLayers.erase(toRemove);
        with_other_side_update_suppressed(layer,
        [&]()
        {
            if (_type == connection_type::output)
            {
                layer->input_connections().remove(_connectedLayer);
            }
            else
            {
                layer->output_connections().remove(_connectedLayer);
            }
        });
        return true;
    }
    return false;
}

void layer_connections::clear()
{
    for (auto& layer : _otherLayers)
    {
        if (_type == connection_type::output)
        {
            layer.second->input_connections().remove(_connectedLayer);
        }
        else
        {
            layer.second->output_connections().remove(_connectedLayer);
        }
    }
    _otherLayers.clear();
}

layers_t layer_connections::connected_layers(flow_direction direction) const
{
    return _otherLayers 
        | where([=](const other_layer_t& layer) { return int(layer.first & direction) != 0; })
        | select([](const other_layer_t& layer) { return layer.second; });
}

void layer_connections::with_other_side_update_suppressed(const layer_ptr& layer, const std::function<void()>& method)
{
    if (_suppressOtherSideUpdate)
    {
        layer->input_connections()._suppressOtherSideUpdate = true;
        layer->output_connections()._suppressOtherSideUpdate = true;
        try
        {
            finally f([&]()
            {
                layer->input_connections()._suppressOtherSideUpdate = false;
                layer->output_connections()._suppressOtherSideUpdate = false;
            });

            method();
        }
        catch (...)
        {
            throw;
        }
    }
}