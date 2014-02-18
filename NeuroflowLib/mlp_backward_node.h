#pragma once

#include "nfdev.h"
#include "activation_description.h"
#include "weighted_errors.h"
#include "supervised_outputs.h"

namespace nf
{
    struct mlp_backward_node
    {
        activation_description activation;
        std::vector<get_device_array_ptr_t> in;
        std::vector<device_array2_ptr> gradients;
        std::vector<device_array2_ptr> gradient_sums;
        get_device_array_ptr_t out;
        device_array_ptr bias_gradients;
        device_array_ptr bias_gradient_sums;
        std::vector<weighted_errors> lower_errors;
        boost::optional<supervised_outputs> net_outputs;

        idx_t size() const;
        bool has_gradients() const;
        bool has_gradient_sums() const;
        bool is_last() const;
    };
}