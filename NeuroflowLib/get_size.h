#pragma once

#include "nf.h"
#include <boost/mpl/assert.hpp>

namespace nf
{
    template <typename T>
    struct get_size
    {
        BOOST_MPL_ASSERT_MSG(false, SPECIALIZATION_EXPECTED, (T));

        inline static unsigned get(T obj)
        {
            return 0;
        }
    };

    template <>
    struct get_size<int>
    {
        inline static unsigned get(int size)
        {
            return (unsigned)size;
        }
    };

    template <>
    struct get_size<unsigned>
    {
        inline static unsigned get(unsigned size)
        {
            return (unsigned)size;
        }
    };
}