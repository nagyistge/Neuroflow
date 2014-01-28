#pragma once

#include "linqlike_base.h"

namespace linqlike
{
    template <typename F = _dummy>
    struct _any
    {
        _any() { }
        _any(const F& pred) : _pred(pred) { }

        const boost::optional<F>& pred() const
        {
            return _pred;
        }

    private:
        boost::optional<F> _pred;
    };

    inline _any<> any()
    {
        return _any<>();
    }

    template <typename F>
    _any<F> any(const F& pred)
    {
        return _any<F>(pred);
    }

    template <typename TColl>
    bool operator|(TColl& coll, const _any<_dummy>& a)
    {
#if (_MSC_VER && _DEBUG)
        bool found = false;
        for (auto& v : coll) if (!found) found = true;
        return found;
#else
        return std::begin(coll) != std::end(coll);
#endif
    }

    template <typename TColl, typename F>
    bool operator|(TColl& coll, const _any<F>& a)
    {
        auto& pred = *a.pred();
#if (_MSC_VER && _DEBUG)
        bool found = false;
        for (auto& v : coll) if (pred(v) && !found) found = true;
        return found;
#else
        for (auto& v : coll) if (pred(v)) return true;
        return false;
#endif
    }
}