using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuroflow.Networks.Neural;
using Neuroflow.Networks.Neural.CPU;

namespace Neuroflow.UnitTests
{
    [TestClass]
    public class NeuralNetworkArchTests
    {
        [TestMethod]
        public void CanCreateSMLCPUTest()
        {
            var arch = new StandardMultilayerArchitecture
            {
                InitParameters = new CPUNNInitParameters(),
                InputSize = 10,
                HiddenLayers = new[] 
                { 
                    new ActivationLayer(10, new SigmoidActivationFunction()) 
                },
                OutputLayer = new ActivationLayer(2, new LinearActivationFunction())
            };

            var nn = arch.CreateNetwork();
            var cpuNN = nn as CPUNeuralNetwork;

            Assert.IsNotNull(nn);
            Assert.IsNotNull(cpuNN);
        }
    }
}
