using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuroflow.Networks.Neural.OpenCL;
using Neuroflow.Networks.Neural;

namespace Neuroflow.UnitTests
{
    [TestClass]
    public class OpenCLNNTests
    {
        [TestMethod]
        public void CreateNNTest()
        {
            var arch = new StandardMultilayerArchitecture
            {
                InitParameters = new OpenCLNNInitParameters
                {
                },
                InputSize = 10,
                HiddenLayers = new[] 
                { 
                    new ActivationLayer(10, new SigmoidActivationFunction()) 
                },
                OutputLayer = new ActivationLayer(2, new LinearActivationFunction())
            };

            using (var nn = (OpenCLNeuralNetwork)arch.CreateNetwork())
            {
                Assert.IsNotNull(nn);

                float[] inputs = new float[10];
                float[] outputs = new float[2];

                Fill(inputs, 1.0f);

                nn.WriteInput(inputs);

                nn.ReadOutput(outputs);
            }
        }

        private static void Fill<T>(T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++) array[i] = value;
        }
    }
}
