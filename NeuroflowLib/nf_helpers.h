#pragma once

#include <ppltasks.h>

namespace nf
{
    template <typename T>
    concurrency::task<T> create_task_from_value(T value)
    {
        using namespace concurrency;
        auto comp = task_completion_event<T>();
        comp.set(value);
        return task<T>(comp);
    }

    concurrency::task<void> create_do_nothing_task()
    {
        using namespace concurrency;
        auto comp = task_completion_event<void>();
        comp.set();
        return task<void>(comp);
    }
}