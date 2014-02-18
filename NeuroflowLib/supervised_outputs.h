#pragma once

#include "nfdev.h"

namespace nf
{
    struct supervised_outputs
    {
        supervised_outputs(const get_device_array_ptr_t& netOutputs, const get_device_array_ptr_t& netDesiredOutputs) :
            _netOutputs(netOutputs),
            _netDesiredOutputs(netDesiredOutputs)
        {
        }

        const get_device_array_ptr_t& net_outputs() const
        {
            return _netOutputs;
        }

        const get_device_array_ptr_t& net_desired_outputs() const
        {
            return _netDesiredOutputs;
        }

    private:
        get_device_array_ptr_t _netOutputs;
        get_device_array_ptr_t _netDesiredOutputs;
    };
}