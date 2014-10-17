#pragma once

#include "Typedefs.h"
#include "Disposable.h"
#include <vector>
#include <amp.h>
#include "IVectorBuffer.h"
#include "StagingView.h"

typedef std::unique_ptr<concurrency::array<float>> CPPAmpVectorBufferItem;

typedef std::vector<CPPAmpVectorBufferItem> CPPAmpVectorBufferRow;

class CPPAmpVectorBuffer : public Disposable, public IVectorBuffer
{
    std::vector<CPPAmpVectorBufferRow> matrix;
    concurrency::accelerator_view accView;

public:
    CPPAmpVectorBuffer(const concurrency::accelerator_view& accView) :
        accView(accView)
    {
    }

    ~CPPAmpVectorBuffer()
    {
    }

    virtual int Create(int rowIndex, int colIndex, const std::function<std::vector<float> ()>& valuesFactory) override;

    concurrency::array<float>& GetArray(const VectorHandle& vector);

private:
    CPPAmpVectorBufferRow& GetRow(int rowIndex);

    void EnsureColSize(int colIndex, CPPAmpVectorBufferRow& row);

    int GetSize(const CPPAmpVectorBufferItem& item);
};

