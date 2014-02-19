#pragma once

#include "nfdev.h"

namespace nf
{
    struct supervised_outputs
    {
        supervised_outputs(const get_device_array_ptr_t& Outputs, const get_device_array_ptr_t& DesiredOutputs) :
            _outputs(Outputs),
            _desiredOutputs(DesiredOutputs)
        {
        }

        const get_device_array_ptr_t& outputs() const
        {
            return _outputs;
        }

        const get_device_array_ptr_t& desired_outputs() const
        {
            return _desiredOutputs;
        }

    private:
        get_device_array_ptr_t _outputs;
        get_device_array_ptr_t _desiredOutputs;
    };
}