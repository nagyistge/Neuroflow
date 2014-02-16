#pragma once

#include <boost/thread.hpp>

namespace nf
{
    template <typename T>
    boost::shared_future<T> create_task_from_value(T value)
    {
        using namespace boost;
        auto comp = promise<T>();
        comp.set_value(value);
        return comp.get_future();
    }

    inline boost::shared_future<void> create_do_nothing_task()
    {
        using namespace boost;
        auto comp = promise<void>();
        comp.set_value();
        return comp.get_future();
    }

    template<typename T>
    T nfmin(T value1, T value2)
    {
        return value1 < value2 ? value1 : value2;
    }

    template<typename T>
    T nfmax(T value1, T value2)
    {
        return value1 > value2 ? value1 : value2;
    }
}