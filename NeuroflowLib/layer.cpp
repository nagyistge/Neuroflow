#include "stdafx.h"
#include "layer.h"

USING;

layer::layer(idx_t size) :
_inputConnections(layer_connections::connection_type::input),
_outputConnections(layer_connections::connection_type::output)
{
}

layer_description_coll& layer::descriptions()
{
    return _descriptions;
}

layer_behavior_coll& layer::behaviors()
{
    return _behaviors;
}

layer_connections& layer::input_connections()
{
    if (_inputConnections.must_init()) _inputConnections.init(shared_this<layer>());
    return _inputConnections;
}

layer_connections& layer::output_connections()
{
    if (_outputConnections.must_init()) _outputConnections.init(shared_this<layer>());
    return _outputConnections;
}

bool layer::has_recurrent_connections() const
{
    bool has = false;
    _inputConnections.visit_connected_layers(flow_direction::one_way_to_source | flow_direction::two_way,
    [&](const layer_ptr& layer)
    {
        has = true;
        return false;
    });
    if (has) return true;
    _outputConnections.visit_connected_layers(flow_direction::one_way_to_source | flow_direction::two_way,
    [&](const layer_ptr& layer)
    {
        has = true;
        return false;
    });
    return has;
}

void layer::visit_input_layers(const layer_visitor_func& visitor) const
{
    bool cont;
    _inputConnections.visit_connected_layers(flow_direction::one_way | flow_direction::two_way, [&](const layer_ptr& layer){ return cont = visitor(layer); });
    if (cont) _outputConnections.visit_connected_layers(flow_direction::two_way | flow_direction::one_way_to_source, [&](const layer_ptr& layer){ return cont = visitor(layer); });
}

void layer::visit_output_layers(const layer_visitor_func& visitor) const
{
    bool cont;
    _outputConnections.visit_connected_layers(flow_direction::one_way | flow_direction::two_way, [&](const layer_ptr& layer){ return cont = visitor(layer); });
    if (cont) _inputConnections.visit_connected_layers(flow_direction::two_way | flow_direction::one_way_to_source, [&](const layer_ptr& layer){ return cont = visitor(layer); });
}

layer_ptr layer::get_input_layer(idx_t connectionIndex) const
{
    int idx = 0;
    layer_ptr result;
    visit_input_layers(
    [&](const layer_ptr& layer)
    {
        if (idx++ == connectionIndex)
        {
            result = layer;
            return false;
        }
        return true;
    });
    if (!result) throw_logic_error("Input layer not found, connection index value " + to_string(connectionIndex) + " was out of range.");
    return result;
}

layer_ptr layer::get_output_layer(idx_t connectionIndex) const
{
    int idx = 0;
    layer_ptr result;
    visit_output_layers(
    [&](const layer_ptr& layer)
    {
        if (idx++ == connectionIndex)
        {
            result = layer;
            return false;
        }
        return true;
    });
    if (!result) throw_logic_error("Output layer not found, connection index value " + to_string(connectionIndex) + " was out of range.");
    return result;
}