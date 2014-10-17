#pragma once

#include <amp.h>
#include "StagingView.h"
#include "IntRange.h"
#include "Disposable.h"

class CPPAmpValueBuffer : public Disposable
{
    concurrency::accelerator_view accView, cpuAccView;

    concurrency::array<float> buff, output;
    concurrency::array_view<float> iSec, oSec;

public:
    CPPAmpValueBuffer(const concurrency::accelerator_view & accView, size_t size, const IntRange& inputRange, const IntRange& outputRange);

    ~CPPAmpValueBuffer() { }

    void Wait();

    void ZeroAll();

    void Zero(const IntRange& range);

    void Fill(const IntRange& range, float value);

    void WriteInput(concurrency::array<float>& values);

    void ReadOutput(float* values);
};

