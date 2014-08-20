#pragma once

#include "nfdev.h"

namespace nf
{
    struct training_node
    {
        training_node(
            const device_array_ptr& weights,
            const device_array_ptr& gradients,
            const device_array_ptr& gradientSums) :
            _weights(weights),
            _gradients(gradients),
            _gradientSums(gradientSums)
        {
        }

        const device_array_ptr& weights() const
        {
            return _weights;
        }

        const device_array_ptr& gradients() const
        {
            return _gradients;
        }

        const device_array_ptr& gradient_sums() const
        {
            return _gradientSums;
        }

    private:
        device_array_ptr _weights;
        device_array_ptr _gradients;
        device_array_ptr _gradientSums;
    };
}