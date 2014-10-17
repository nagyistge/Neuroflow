#pragma once

#include <functional>

namespace nf
{
    struct finally
    {
        finally(const std::function<void()>& f) : _f(f) { }
        ~finally() { _f(); }

    private:
        std::function<void()> _f;
    };
}