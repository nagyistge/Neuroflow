#pragma once

#include "nfdev.h"

namespace nf
{
    struct weighted_errors
    {
        weighted_errors(const device_array_ptr& errors, const device_array2_ptr& weights) :
            _errors(errors),
            _weights(weights)
        {
        }

        const device_array_ptr& errors() const
        {
            return _errors;
        }

        const device_array2_ptr& weights() const
        {
            return _weights;
        }

    private:
        device_array_ptr _errors;
        device_array2_ptr _weights;
    };
}