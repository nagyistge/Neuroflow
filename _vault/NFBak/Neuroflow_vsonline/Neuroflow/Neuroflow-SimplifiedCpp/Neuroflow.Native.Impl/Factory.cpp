#include "stdafx.h"
#include "Factory.h"
#include "CPPAmpNN.h"

INativeNeuralNetworkImpl* Factory::CreateCPPAmpNN()
{
    return new CPPAmpNN();
}
