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
    return (from(_inputConnections.connected_layers(flow_direction::one_way_to_source | flow_direction::two_way)) >> any())
        || (from(_outputConnections.connected_layers(flow_direction::one_way_to_source | flow_direction::two_way)) >> any());
}

layers_t layer::input_layers(const layer_visitor_func& visitor) const
{
    return _inputConnections.connected_layers(flow_direction::one_way | flow_direction::two_way)
        >> concat(_outputConnections.connected_layers(flow_direction::two_way | flow_direction::one_way_to_source));
}

layers_t layer::output_layers(const layer_visitor_func& visitor) const
{
    return _outputConnections.connected_layers(flow_direction::one_way | flow_direction::two_way)
        >> concat(_inputConnections.connected_layers(flow_direction::two_way | flow_direction::one_way_to_source));
}

layer_ptr layer::get_input_layer(idx_t connectionIndex) const
{
    /*int idx = 0;
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
    return result;*/
    return null;
}

layer_ptr layer::get_output_layer(idx_t connectionIndex) const
{
    /*int idx = 0;
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
    return result;*/
    return null;
}