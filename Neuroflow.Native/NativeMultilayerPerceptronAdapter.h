#pragma once

#include "Typedefs.h"
#include <assert.h>

namespace Neuroflow
{
    namespace NeuralNetworks
    {
        ref class NativeMultilayerPerceptronAdapter : public Neuroflow::NeuralNetworks::IMultilayerPerceptronAdapter
        {
            NeuroflowN::IMultilayerPerceptronAdapter* adapter;

            Neuroflow::IDeviceArrayManagement^ deviceArrayManagement;

            Neuroflow::VectorUtils^ vectorUtils;

            Neuroflow::NeuralNetworks::IComputeActivation^ computeActivation;

            Neuroflow::NeuralNetworks::ILearningAlgoFactory^ learningAlgoFactory;

        public:
            NativeMultilayerPerceptronAdapter(NeuroflowN::IMultilayerPerceptronAdapter* adapter);

            virtual property Neuroflow::IDeviceArrayManagement^ DeviceArrayManagement
            {
                Neuroflow::IDeviceArrayManagement^ get()
                {
                    return deviceArrayManagement;
                }
            }

            virtual property Neuroflow::VectorUtils^ VectorUtils 
            {
                Neuroflow::VectorUtils^ get()
                {
                    return vectorUtils;
                }
            }

            virtual property Neuroflow::NeuralNetworks::IComputeActivation^ ComputeActivation 
            {
                Neuroflow::NeuralNetworks::IComputeActivation^ get()
                {
                    return computeActivation;
                }
            }

            virtual property Neuroflow::NeuralNetworks::ILearningAlgoFactory^ LearningAlgoFactory 
            {
                Neuroflow::NeuralNetworks::ILearningAlgoFactory^ get()
                {
                    return learningAlgoFactory;
                }
            }
        };
    }    
}