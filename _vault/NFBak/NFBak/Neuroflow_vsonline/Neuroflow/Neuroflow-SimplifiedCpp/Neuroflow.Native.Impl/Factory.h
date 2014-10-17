#pragma once

#include "INativeNeuralNetworkImpl.h"
#include "IntRange.h"
#include "LayerForwardCompute.h"

class Factory
{
public:
    static INativeNeuralNetworkImpl* CreateCPPAmpNN();
};

