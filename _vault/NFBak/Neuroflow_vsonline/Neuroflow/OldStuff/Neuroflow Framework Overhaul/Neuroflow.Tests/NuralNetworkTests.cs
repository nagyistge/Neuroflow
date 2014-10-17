using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Pex.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuroflow.Networks.Neural;
using Neuroflow.Core;

namespace Neuroflow.Tests
{
    [TestClass, PexClass]
    public partial class NuralNetworkTests
    {
        [TestMethod]
        public void CreateWiredNetwork()
        {
            CreateWiredNetwork(10, 10, 10);
        }
        
        [PexMethod]
        public void CreateWiredNetwork(int iil, int oil, int nc)
        {
            PexAssume.InRange(iil, 1, 41);
            PexAssume.InRange(oil, 1, 41);
            PexAssume.InRange(nc, 1, 41);

            var a = new WiredArchitecture(
                iil,
                oil,
                nc,
                new Factory<NeuralNode>(() => new ActivationNeuron(new SigmoidActivationFunction(1.05))),
                new Factory<NeuralNode>(() => new ActivationNeuron(new LinearActivationFunction(1.05))),
                new Factory<NeuralConnection>(() => new Synapse()));

            var net = a.CreateNetwork();
            net.Iteration();
        }

        [TestMethod]
        public void CreateStandardMLANetwork()
        {
            CreateStandardMLANetwork(10, 40, 40, 2);
        }

        [PexMethod]
        public void CreateStandardMLANetwork(int iil, int oil, int nc, int nl)
        {
            PexAssume.InRange(iil, 1, 41);
            PexAssume.InRange(oil, 1, 41);
            PexAssume.InRange(nc, 1, 41);
            PexAssume.InRange(nl, 2, 41);

            var nodeF = new Factory<NeuralNode>(() => new ActivationNeuron(new SigmoidActivationFunction(1.05)));
            var connF = new Factory<NeuralConnection>(() => new Synapse());

            var layers = new NeuralLayerDefinition[nl];

            for (int i = 0; i < nl; i++)
            {
                if (i < (nl - 1))
                {
                    layers[i] = new NeuralLayerDefinition(nodeF, nc);
                }
                else
                {
                    layers[i] = new NeuralLayerDefinition(nodeF, oil);
                }
            }

            var a = new StandardMultilayerArchitecture(new NeuralConnectionDefinition(connF, false), iil, layers);

            var net = a.CreateNetwork();
            net.Iteration();
        }
    }
}
