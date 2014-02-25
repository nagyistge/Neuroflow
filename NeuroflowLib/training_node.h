#pragma once

#include "nfdev.h"

namespace nf
{
    struct training_node
    {
        training_node(
            device_array_collection_t&& weights,
            boost::optional<device_array_collection_t>&& gradients,
            boost::optional<device_array_collection_t> gradientSums) :
            _weights(std::move(weights)),
            _gradients(std::move(gradients)),
            _gradientSums(std::move(gradientSums))
        {
        }

        const device_array_collection_t weights() const
        {
            return _weights;
        }

        const boost::optional<device_array_collection_t> gradients() const
        {
            return _gradients;
        }

        const boost::optional<device_array_collection_t> gradient_sums() const
        {
            return _gradientSums;
        }

    private:
        device_array_collection_t _weights;
        boost::optional<device_array_collection_t> _gradients;
        boost::optional<device_array_collection_t> _gradientSums;
    };
}