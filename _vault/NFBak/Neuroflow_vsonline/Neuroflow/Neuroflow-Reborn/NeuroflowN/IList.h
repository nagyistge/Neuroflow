#pragma once

#include "NfObject.h"

namespace NeuroflowN
{
    template<typename T>
    class IList : public NfObject
    {
    public:
        virtual unsigned GetCount() const = 0;

        virtual T operator[](const unsigned index) const = 0;
    };
}

