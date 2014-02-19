#pragma once

#include <boost/thread.hpp>

namespace nf
{
    template <typename T>
    inline boost::shared_future<T> create_task_from_value(T value)
    {
        using namespace boost;
        return make_ready_future<T>(value).share();
    }

    inline boost::shared_future<void> create_do_nothing_task()
    {
        using namespace boost;
        auto comp = promise<void>();
        comp.set_value();
        return comp.get_future();
    }

    template<typename T>
    inline T nfmin(T value1, T value2)
    {
        return value1 < value2 ? value1 : value2;
    }

    template<typename T>
    inline T nfmax(T value1, T value2)
    {
        return value1 > value2 ? value1 : value2;
    }

    template<typename T>
    inline T get_index2(T i1, T i2, T size1)
    {
        return i2 * size1 + i1;
    }
}