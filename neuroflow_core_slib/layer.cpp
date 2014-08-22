#include "stdafx.h"
#include "layer.h"
#include "row_numbered.h"

USING

layer::layer(idx_t size) :
_size(size),
_inputConnections(layer_connections::connection_type::input),
_outputConnections(layer_connections::connection_type::output)
{
}

idx_t layer::size() const
{
    return _size;
}

layer_description_coll_t& layer::descriptions()
{
    return _descriptions;
}

layer_behavior_coll_t& layer::behaviors()
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
    return from(_inputConnections.connected_layers(flow_direction::one_way_to_source | flow_direction::two_way)) >> any()
        || from(_outputConnections.connected_layers(flow_direction::one_way_to_source | flow_direction::two_way)) >> any();
}

layer_collection_t layer::input_layers() const
{
    layer_collection_t result;
    auto l1 = _inputConnections.connected_layers(flow_direction::one_way | flow_direction::two_way);
    auto l2 = _outputConnections.connected_layers(flow_direction::two_way | flow_direction::one_way_to_source);
    result.insert(result.end(), l1.begin(), l1.end());
    result.insert(result.end(), l2.begin(), l2.end());
    return move(result);
}

layer_collection_t layer::output_layers() const
{
    layer_collection_t result;
    auto l1 = _outputConnections.connected_layers(flow_direction::one_way | flow_direction::two_way);
    auto l2 = _inputConnections.connected_layers(flow_direction::two_way | flow_direction::one_way_to_source);
    result.insert(result.end(), l1.begin(), l1.end());
    result.insert(result.end(), l2.begin(), l2.end());
    return move(result);
}

layer_ptr layer::get_input_layer(idx_t connectionIndex) const
{
    idx_t idx = 0;
    auto result = from(input_layers())
        >> select([&](const layer_ptr& l){ return row_numbered<layer_ptr>(idx++, l); })
        >> where([=](const row_numbered<layer_ptr>& obj) { return obj.row_num() == connectionIndex; })
        >> select([](const row_numbered<layer_ptr>& obj) { return obj.value(); })
        >> first_or_default();

    if (!result) throw_logic_error("Input layer not found, connection index value " + to_string(connectionIndex) + " was out of range.");
    return result;
}

layer_ptr layer::get_output_layer(idx_t connectionIndex) const
{
    idx_t idx = 0;
    auto result = from(output_layers())
        >> select([&](const layer_ptr& l){ return row_numbered<layer_ptr>(idx++, l); })
        >> where([=](const row_numbered<layer_ptr>& obj) { return obj.row_num() == connectionIndex; })
        >> select([](const row_numbered<layer_ptr>& obj) { return obj.value(); })
        >> first_or_default();

    if (!result) throw_logic_error("Output layer not found, connection index value " + to_string(connectionIndex) + " was out of range.");
    return result;
}
