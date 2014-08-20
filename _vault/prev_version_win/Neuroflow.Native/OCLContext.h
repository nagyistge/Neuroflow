#pragma once

#include "Typedefs.h"
#include "OCLTypedefs.h"

namespace Neuroflow
{
    public ref class OCLContext : public ComputationContext
    {
        NeuroflowN::OCLContextImpl * oclContext;
        Data::DataArrayFactory^ dataArrayFactory;
        Neuroflow::VectorUtils^ vectorUtils;
        NeuralNetworks::IMultilayerPerceptronAdapter^ multilayerPerceptronAdapter;

    public:
        OCLContext(System::String^ deviceID);

        virtual property Neuroflow::Data::DataArrayFactory^ DataArrayFactory
        {
            Data::DataArrayFactory^ get() override
            {
                return dataArrayFactory;
            }
        }

        virtual property NeuralNetworks::IMultilayerPerceptronAdapter^ MultilayerPerceptronAdapter
        {
            NeuralNetworks::IMultilayerPerceptronAdapter^ get() override
            {
                return multilayerPerceptronAdapter;
            }
        }

        virtual property Neuroflow::VectorUtils^ VectorUtils
        {
            Neuroflow::VectorUtils^ get() override
            {
                return vectorUtils;
            }
        }

        virtual property Neuroflow::Device Device
        {
            Neuroflow::Device get() override
            {
                return GetDevice();
            }
        }

        static array<Neuroflow::Device>^ GetAvailableDevices();

    protected:
        virtual void CleanupNativeResources() override;

        virtual void WaitForFinish(ComputationFinishedCallback^ callback) override;

    private:
        Neuroflow::Device GetDevice();
    };
}