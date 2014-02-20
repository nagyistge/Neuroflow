#include "stdafx.h"
#include "CPPAmpValueBuffer.h"
#include "IntRange.h"

using namespace concurrency;

CPPAmpValueBuffer::CPPAmpValueBuffer(const accelerator_view & accView, size_t size, const IntRange& inputRange, const IntRange& outputRange) :
    accView(accView),
    cpuAccView(accelerator(accelerator::cpu_accelerator).default_view),
    buff(array<float>(size, accView)),
    output(array<float>(outputRange.Size, cpuAccView, accView)),
    iSec(buff.section(inputRange.MinValue, inputRange.Size)),
    oSec(buff.section(outputRange.MinValue, outputRange.Size))
{
}

void CPPAmpValueBuffer::Wait()
{
    accView.wait();
}

void CPPAmpValueBuffer::ZeroAll()
{
    auto& buff = this->buff;
    parallel_for_each(buff.extent, [&buff] (index<1> idx) restrict(amp)
    {
        buff[idx] = 0.0f;
    });
}

void CPPAmpValueBuffer::Zero(const IntRange& range)
{
    auto& buff = this->buff;
    int min = range.MinValue;
    parallel_for_each(concurrency::extent<1>(range.Size), [=, &buff] (index<1> idx) restrict(amp)
    {
        buff[min + idx] = 0.0f;
    });
}

void CPPAmpValueBuffer::Fill(const IntRange& range, float value)
{
    auto& buff = this->buff;
    int min = range.MinValue;
    parallel_for_each(concurrency::extent<1>(range.Size), [=, &buff] (index<1> idx) restrict(amp)
    {
        buff[min + idx] = value;
    });
}

void CPPAmpValueBuffer::WriteInput(array<float>& values)
{
    auto& iSec = this->iSec;
    copy(values, iSec);
}

void CPPAmpValueBuffer::ReadOutput(float* values)
{
    auto& oSec = this->oSec;
    copy(oSec, output);
    for (int i = 0; i < (int)output.extent.size(); i++) values[i] = output[i];
}
