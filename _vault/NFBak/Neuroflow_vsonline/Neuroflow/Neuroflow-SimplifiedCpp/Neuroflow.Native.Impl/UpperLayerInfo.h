#pragma once

class UpperLayerInfo
{
public:

    UpperLayerInfo() :
        weightedErrorBufferIndex(0),
        layerIndex(0)
    {
    }

    UpperLayerInfo(int weightedErrorBufferIndex, int layerIndex) : 
        weightedErrorBufferIndex(weightedErrorBufferIndex),
        layerIndex(layerIndex)
    {
    }

    UpperLayerInfo(const UpperLayerInfo& other) : 
        weightedErrorBufferIndex(other.weightedErrorBufferIndex),
        layerIndex(other.layerIndex)
    {
    }

private:
    int weightedErrorBufferIndex;
public:
    const int GetWeightedErrorBufferIndex() const { return weightedErrorBufferIndex; } 
    __declspec(property(get = GetWeightedErrorBufferIndex)) int WeightedErrorBufferIndex;

private:
    int layerIndex;
public:
    const int GetLayerIndex() const { return layerIndex; } 
    __declspec(property(get = GetLayerIndex)) int LayerIndex;
};

