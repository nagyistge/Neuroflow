#pragma once

#include "Typedefs.h"
#include "NativePtr.h"
#include <assert.h>

namespace Neuroflow
{
    ref class NativeObject : NativePtr<NeuroflowN::NfObject>
    {
    public:
        NativeObject(NeuroflowN::NfObject* obj) :
            NativePtr(obj)
        {
            assert(obj != null);
        }
    };
}

