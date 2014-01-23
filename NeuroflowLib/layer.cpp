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
    return _inputConnections.connected_layers(flow_direction::one_way_to_source | flow_direction::two_way) | any()
        || _outputConnections.connected_layers(flow_direction::one_way_to_source | flow_direction::two_way) | any();
}

layers_t layer::input_layers() const
{
    return _inputConnections.connected_layers(flow_direction::one_way | flow_direction::two_way)
        | concat(_outputConnections.connected_layers(flow_direction::two_way | flow_direction::one_way_to_source));
}

layers_t layer::output_layers() const
{
    return _outputConnections.connected_layers(flow_direction::one_way | flow_direction::two_way)
        | concat(_inputConnections.connected_layers(flow_direction::two_way | flow_direction::one_way_to_source));
}

layer_ptr layer::get_input_layer(idx_t connectionIndex) const
{
    auto result = input_layers()
        | row_num()
        | where([=](row_numbered<layer_ptr>& obj) { return obj.row_num() == connectionIndex; })
        | select([](row_numbered<layer_ptr>& obj) { return obj.value(); })
        | first_or_default();

    if (!result) throw_logic_error("Input layer not found, connection index value " + to_string(connectionIndex) + " was out of range.");
    return result;
}

layer_ptr layer::get_output_layer(idx_t connectionIndex) const
{
    auto result = output_layers()
        | row_num()
        | where([=](row_numbered<layer_ptr>& obj) { return obj.row_num() == connectionIndex; })
        | select([](row_numbered<layer_ptr>& obj) { return obj.value(); })
        | first_or_default();

    if (!result) throw_logic_error("Output layer not found, connection index value " + to_string(connectionIndex) + " was out of range.");
    return result;
}