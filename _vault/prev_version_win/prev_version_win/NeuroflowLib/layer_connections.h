#pragma once

#include "nfdev.h"

namespace nf
{
    struct layer_connections
    {
        friend struct layer;

        void add_one_way(const layer_ptr& layer);
        void add_two_way(const layer_ptr& layer);
        void add_one_way_to_source(const layer_ptr& layer);
        void add(const layer_ptr& layer, flow_direction direction);
        bool remove(const layer_ptr& layer);
        void clear();
        layers_t connected_layers(flow_direction direction) const;

    private:
        enum class connection_type { input, output };

        layer_connections(layer_connections::connection_type type);

        std::list<other_layer_t> _otherLayers;
        bool _suppressOtherSideUpdate = false;
        connection_type _type;
        layer_ptr _connectedLayer;

        void init(const layer_ptr& connectedLayer);
        bool must_init() const;
        void with_other_side_update_suppressed(const layer_ptr& layer, const std::function<void()>& method);
    };
}