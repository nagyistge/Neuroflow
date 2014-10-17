#pragma once

class IntRange
{
private:
    int minValue, maxValue;

public:
    IntRange() : minValue(0), maxValue(0) { }

    IntRange(int minValue, int maxValue) : minValue(minValue), maxValue(maxValue) { }

    IntRange(const IntRange& other) : minValue(other.minValue), maxValue(other.maxValue) { }

    const int GetMinValue() const { return minValue; } 
    __declspec(property(get = GetMinValue)) int MinValue;

    const int GetMaxValue() const { return maxValue; } 
    __declspec(property(get = GetMaxValue)) int MaxValue;    

    const bool GetIsEmpty() const { return minValue == maxValue; } 
    __declspec(property(get = GetIsEmpty)) bool IsEmpty;

    const int GetSize() const { return maxValue - minValue + 1; } 
    __declspec(property(get = GetSize)) int Size;
};

