#pragma once

#include "nfdev.h"

namespace nf
{
    struct weighted_inputs
    {
        weighted_inputs(const get_device_array_ptr_t& inputs, const device_array2_ptr& weights) :
            _inputs(inputs),
            _weights(weights)
        {
        }

        const get_device_array_ptr_t& inputs() const
        {
            return _inputs;
        }

        const device_array2_ptr& weights() const
        {
            return _weights;
        }

    private:
        get_device_array_ptr_t _inputs;
        device_array2_ptr _weights;
    };
}