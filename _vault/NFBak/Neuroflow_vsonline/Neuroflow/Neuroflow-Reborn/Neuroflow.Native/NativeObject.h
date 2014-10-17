#pragma once

#include "Typedefs.h"
#include <assert.h>

namespace Neuroflow
{
    ref class NativeObject : DisposableObject
    {
        NeuroflowN::NfObject* obj;

    public:
        NativeObject(NeuroflowN::NfObject* obj) :
            obj(obj)
        {
            assert(obj != null);
        }

        property NeuroflowN::NfObject* PObj
        {
            NeuroflowN::NfObject * get()
            {
                return obj;
            }
        }
    };
}

