#include "stdafx.h"
#include "LayerForwardCompute.h"
#include "CPPAmpValueBuffer.h"

using namespace concurrency;

LayerForwardCompute::LayerForwardCompute(void) : 
    connectedLayerIndex(0),
    isOutput(false),
    biasValueIndex(-1),
    method(ForwardComputationMethod::FeedForward),
    biasGradientValueIndex(-1),
    biasGradientSumValueIndex(-1)
{
}

void LayerForwardCompute::Reset(CPPAmpValueBuffer& buffer, NeuralNetworkResetTarget target)
{
    if (((int)target & (int)NeuralNetworkResetTarget::Outputs) != 0)
    {
        buffer.Zero(OutputBuffer);
    }
    else if (((int)target & (int)NeuralNetworkResetTarget::Ps) != 0)
    {
        if (Method == ForwardComputationMethod::RTLR)
        {
            for each (auto range in PBiasBuffers)
            {
                buffer.Zero(range);
            }

            for each (auto lvl1 in PWeightBuffers)
            {
                for each (auto lvl2 in lvl1)
                {
                    for each (auto range in lvl2)
                    {
                        buffer.Zero(range);
                    }
                }
            }
        }
    }
}
