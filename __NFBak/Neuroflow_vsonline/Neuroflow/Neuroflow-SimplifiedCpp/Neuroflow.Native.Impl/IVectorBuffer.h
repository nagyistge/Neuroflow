#pragma once

#include <functional>
#include <vector>

__interface IVectorBuffer
{
public:
    virtual int Create(int rowIndex, int colIndex, const std::function<std::vector<float> ()>& valuesFactory) = 0;
};

