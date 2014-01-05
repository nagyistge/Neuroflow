#pragma once

#include "nfdev.h"
#include "layer_connections.h"

namespace nf
{
    struct layer : virtual nf_object
    {
        layer(idx_t size);

        layer_description_coll& descriptions();
        layer_behavior_coll& behaviors();
        layer_connections& input_connections();
        layer_connections& output_connections();
        bool has_recurrent_connections() const;
        void visit_input_layers(const layer_visitor_func& visitor) const;
        void visit_output_layers(const layer_visitor_func& visitor) const;
        layer_ptr get_input_layer(idx_t connectionIndex) const;
        layer_ptr get_output_layer(idx_t connectionIndex) const;

    private:
        layer_description_coll _descriptions;
        layer_behavior_coll _behaviors;
        layer_connections _inputConnections;
        layer_connections _outputConnections;
    };

    inline layer_ptr make_layer(idx_t size)
    {
        return std::make_shared<layer>(size);
    }
}