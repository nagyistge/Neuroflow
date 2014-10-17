#pragma once

#include <amp.h>

template <typename T>
class StagingView
{
    concurrency::array<T> buff;
    concurrency::array_view<T> buffView;

    StagingView() { }
    StagingView(const StagingView& other) { }
public:
    StagingView(const concurrency::accelerator_view& accView, int size) :
        buff(concurrency::array<T>(size, concurrency::accelerator(concurrency::accelerator::cpu_accelerator).default_view, accView)),
        buffView(concurrency::array_view<T>(buff))
    {
    }

    StagingView(const concurrency::accelerator_view& accView, const std::vector<float>& items) :
        buff(concurrency::array<T>(items.size(), items.begin(), concurrency::accelerator(concurrency::accelerator::cpu_accelerator).default_view, accView)),
        buffView(concurrency::array_view<T>(buff))
    {
    }

    concurrency::array_view<T>& GetView() { return buffView; }
    _declspec(property(get = GetView)) concurrency::array_view<T>& View;

    concurrency::array<T>& GetArray() { return buff; }
    _declspec(property(get = GetArray)) concurrency::array<T>& Array;

    const int GetSize() const { return buff.extent.size(); }
    _declspec(property(get = GetSize)) const int Size;
};

