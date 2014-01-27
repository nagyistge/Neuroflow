#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    struct any
    {
    };

    template <typename TColl>
    bool operator|(TColl& coll, const any& a)
    {
#if (_MSC_VER && _DEBUG)
        bool any = false;
        for (auto& v : coll) if (!any) any = true;
        return any;
#else
        return std::begin(coll) != std::end(coll);
#endif

    }
}