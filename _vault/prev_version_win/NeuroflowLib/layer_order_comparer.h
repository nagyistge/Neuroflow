#pragma once

#include "nfdev.h"

namespace nf
{
    struct layer_order_comparer
    {
        bool operator()(const layer_ptr& layer1, const layer_ptr& layer2);

    private:
        std::map<std::pair<layer_ptr, layer_ptr>, int> results;

        int compare(const layer_ptr& layer1, const layer_ptr& layer2);
        bool is_below(const layer_ptr& layer1, const layer_ptr& layer2);
    };
}