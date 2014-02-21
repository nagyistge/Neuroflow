#include "stdafx.h"
#include "CPPAmpVectorBuffer.h"

using namespace std;
using namespace concurrency;

int CPPAmpVectorBuffer::Create(int rowIndex, int colIndex, const function<vector<float> ()>& valuesFactory)
{
    auto& row = GetRow(rowIndex);
    EnsureColSize(colIndex, row);
    if (!row[colIndex])
    {
        auto values = valuesFactory();
        row[colIndex] = move(CPPAmpVectorBufferItem(new array<float>(values.size(), values.begin(), accView)));
    }
    return GetSize(row[colIndex]);
}

array<float>& CPPAmpVectorBuffer::GetArray(const VectorHandle& vector)
{
    register int rowIndex = get<0>(vector);  
    register int colIndex = get<1>(vector); 
    register int length = get<2>(vector); 

    if (rowIndex > (int)matrix.size() - 1) 
    {
        goto Error;
    }
    else
    {
        auto& row = matrix[rowIndex];

        if (colIndex > (int)row.size() - 1) 
        {
            goto Error;
        }
        else
        {
            auto& item = row[colIndex];
            if (item && item->extent.size() == length)
            {
                return *item;
            }
        }
    }
            
Error:
    throw logic_error("Vector is not belongs to this Vector buffer.");
}

CPPAmpVectorBufferRow& CPPAmpVectorBuffer::GetRow(int rowIndex)
{
    if (rowIndex >= (int)matrix.size())
    {
        matrix.resize(rowIndex + 1);
    }
    return matrix[rowIndex];
}

void CPPAmpVectorBuffer::EnsureColSize(int colIndex, CPPAmpVectorBufferRow& row)
{
    if (colIndex >= (int)row.size())
    {
        row.resize(colIndex + 1);
    }
}

int CPPAmpVectorBuffer::GetSize(const CPPAmpVectorBufferItem& item)
{
    if (item)
    {
        return item->extent.size();
    }

    throw logic_error("Vector is not initialized.");
}