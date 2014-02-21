#pragma once

class ErrorValueAccess
{
private:
    int errorSize, errorBufferBeginIndex, weightBufferBeginIndex;
public:
    ErrorValueAccess(int errorSize, int errorBufferBeginIndex, int weightBufferBeginIndex) 
        : errorSize(errorSize),
          errorBufferBeginIndex(errorBufferBeginIndex),
          weightBufferBeginIndex(weightBufferBeginIndex)
    { 
    }

    const int GetErrorSize() const { return errorSize; } 
    __declspec(property(get = GetErrorSize)) int ErrorSize;

    const int GetErrorBufferBeginIndex() const { return errorBufferBeginIndex; } 
    __declspec(property(get = GetErrorBufferBeginIndex)) int ErrorBufferBeginIndex;

    const int GetWeightBufferBeginIndex() const { return weightBufferBeginIndex; } 
    __declspec(property(get = GetWeightBufferBeginIndex)) int WeightBufferBeginIndex;
};

