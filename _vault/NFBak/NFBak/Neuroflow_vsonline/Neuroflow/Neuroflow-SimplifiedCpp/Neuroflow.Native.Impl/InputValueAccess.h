#pragma once

#include "Typedefs.h"

class InputValueAccess
{
private:
    int inputSize, inputBufferBeginIndex, weightBufferBeginIndex;

    std::vector<IntRange> innerItarationInputValueStack;
public:
    InputValueAccess() :
        inputSize(0),
        inputBufferBeginIndex(0),
        weightBufferBeginIndex(0)
    {
    }

    InputValueAccess(int inputSize, int inputBufferBeginIndex, int weightBufferBeginIndex, const std::vector<IntRange>& InnerItarationInputValueStack) 
        : inputSize(inputSize),
          inputBufferBeginIndex(inputBufferBeginIndex),
          weightBufferBeginIndex(weightBufferBeginIndex),
          innerItarationInputValueStack(innerItarationInputValueStack)
    { 
    }

    const int GetInputSize() const { return inputSize; } 
    __declspec(property(get = GetInputSize)) int InputSize;

    const int GetInputBufferBeginIndex() const { return inputBufferBeginIndex; } 
    __declspec(property(get = GetInputBufferBeginIndex)) int InputBufferBeginIndex;

    const int GetWeightBufferBeginIndex() const { return weightBufferBeginIndex; } 
    __declspec(property(get = GetWeightBufferBeginIndex)) int WeightBufferBeginIndex;

    const std::vector<IntRange>& GetinnerItarationInputValueStack() const { return innerItarationInputValueStack; } 
    __declspec(property(get = GetinnerItarationInputValueStack)) std::vector<IntRange>& InnerItarationInputValueStack;
};

