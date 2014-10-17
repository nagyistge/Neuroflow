using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuroflow.NeuralNetworks;
using Neuroflow.Data;
using System.Diagnostics;
using System.Threading;

namespace Neuroflow.UT
{
    [TestClass]
    public class MultilayerPerceptronFFTests
    {
        #region Compute

        [TestMethod]
        [TestCategory(TestCategories.Managed)]
        public async Task ManagedMLPComputeTest()
        {
            using (var ctx = new ManagedContext())
            {
                await MLPComputeTest(ctx);
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        public async Task OCLMLPComputeCPUTest()
        {
            await OCLMLPComputeTest("CPU");
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        public async Task OCLMLPComputeGPUTest()
        {
            await OCLMLPComputeTest("GPU");
        }

        async Task OCLMLPComputeTest(string device)
        {
            using (var ctx = new OCLContext(device))
            {
                await MLPComputeTest(ctx);
            }
        }

        private static async Task MLPComputeTest(ComputationContext ctx)
        {
            const int inputSize = 15;
            const int hiddenSize = 30;
            const int outputSize = 12;
            var inputValues = RandomGenerator.NextFloats(-1.0f, 1.0f, inputSize).ToArray();

            using (var inputDataArray = ctx.DataArrayFactory.Create(inputValues))
            using (var outputDataArray = ctx.DataArrayFactory.Create(outputSize))
            {
                var layers = new[]
                {
                    new Layer(inputSize),
                    new Layer(hiddenSize)
                    {
                        Descriptions =
                        {
                            new ActivationDescription(ActivationFunction.Sigmoid)
                        }
                    },
                    new Layer(outputSize)
                    {
                        Descriptions =
                        {
                            new ActivationDescription(ActivationFunction.Linear)
                        }
                    },
                };

                layers[0].OutputConnections.AddOneWay(layers[1]);
                layers[1].OutputConnections.AddOneWay(layers[2]);

                using (var nn = ctx.NeuralNetworkFactory.CreateMultilayerPerceptron(layers))
                {
                    int numWeights = nn.NumberOfWeights;

                    Assert.AreEqual(layers[1].Size * layers[0].Size + layers[1].Size * layers[2].Size + layers[1].Size + layers[2].Size, numWeights);

                    var rndWeights = RandomGenerator.NextFloats(-1.0f, 1.0f, numWeights).ToArray();

                    using (var tmpWeights = ctx.DataArrayFactory.Create(rndWeights))
                    {
                        nn.SetWeights(tmpWeights);
                    }

                    var readWeights = new float[numWeights];
                    using (var tmpWeights = ctx.DataArrayFactory.Create(numWeights))
                    {
                        nn.GetWeights(tmpWeights);
                        await tmpWeights.Read(readWeights);
                    }

                    for (int i = 0; i < rndWeights.Length; i++)
                    {
                        Assert.AreEqual(rndWeights[i], readWeights[i]);
                    }

                    var readOutputs = new float[outputSize];

                    await outputDataArray.Read(readOutputs);

                    for (int i = 0; i < outputSize; i++)
                    {
                        Assert.AreEqual(0.0f, readOutputs[i]);
                    }

                    var sw = new Stopwatch();
                    sw.Start();

                    nn.Compute(inputDataArray, outputDataArray);

                    await outputDataArray.Read(readOutputs);

                    sw.Stop();
                    Console.WriteLine("Ellapsed: {0} ms", sw.ElapsedMilliseconds);

                    Assert.IsTrue(readOutputs.Any(o => o != 0.0f));

                    var readInputs = new float[inputSize];

                    await inputDataArray.Read(readInputs);

                    for (int i = 0; i < inputSize; i++)
                    {
                        Assert.AreEqual(inputValues[i], readInputs[i]);
                    }
                }
            }
        }

        #endregion

        #region Training

        [TestMethod]
        [TestCategory(TestCategories.Managed)]
        [TestCategory(TestCategories.FF)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task ManagedMLPTrainFFGDOnlineTest()
        {
            using (var ctx = new ManagedContext())
            {
                await MLPTrainFFTest(ctx, GetGDRules(WeigthUpdateMode.Online, 0.1f));
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.Managed)]
        [TestCategory(TestCategories.FF)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task ManagedMLPTrainFFGDOfflineTest()
        {
            using (var ctx = new ManagedContext())
            {
                await MLPTrainFFTest(ctx, GetGDRules(WeigthUpdateMode.Offline, 0.1f));
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        [TestCategory(TestCategories.FF)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task OCLMLPTrainFFGDOnlineCPUTest()
        {
            using (var ctx = new OCLContext("cpu"))
            {
                await MLPTrainFFTest(ctx, GetGDRules(WeigthUpdateMode.Online, 0.1f));
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        [TestCategory(TestCategories.FF)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task OCLMLPTrainFFGDOfflineCPUTest()
        {
            using (var ctx = new OCLContext("cpu"))
            {
                await MLPTrainFFTest(ctx, GetGDRules(WeigthUpdateMode.Offline, 0.1f));
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        [TestCategory(TestCategories.FF)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task OCLMLPTrainFFGDOnlineGPUTest()
        {
            using (var ctx = new OCLContext("Barts"))
            {
                await MLPTrainFFTest(ctx, GetGDRules(WeigthUpdateMode.Online, 0.1f));
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        [TestCategory(TestCategories.FF)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task OCLMLPTrainFFGDOfflineGPUTest()
        {
            using (var ctx = new OCLContext("gpu"))
            {
                await MLPTrainFFTest(ctx, GetGDRules(WeigthUpdateMode.Offline, 0.1f));
            }
        }

        private LayerBehavior[] GetGDRules(WeigthUpdateMode updateMode, float rate)
        {
            var init = new UniformRandomizeWeights(.3f);

            var algo = new GradientDescentLearningRule
            {
                LearningRate = rate,
                Momentum = updateMode == WeigthUpdateMode.Online ? 0.25f : 0.8f,
                WeightUpdateMode = updateMode,
                Smoothing = false
            };

            return new LayerBehavior[] { init, algo };
        }

        private async Task MLPTrainFFTest(ComputationContext ctx, params LayerBehavior[] rules)
        {
            var trainingData =
                new[,]
                {
                    { -4.0f, 16.0f },
                    { -3.0f, 9.0f },
                    { -2.0f, 4.0f },
                    { -1.0f, 1.0f },
                    { 0.0f, 0.0f },
                    { 1.0f, 1.0f },
                    { 2.0f, 4.0f },
                    { 3.0f, 9.0f },
                    { 4.0f, 16.0f },
                };

            const float maxInput = 4.0f;
            const float minInput = -4.0f;
            const float maxOutput = 16.0f;
            const float minOutput = 0.0f;

            const int inputSize = 1;
            const int hiddenSize = 16;
            const int outputSize = 1;

            const int maxIterations = 1000;

            var layers = NNTestHelpers.CreateGDMLPLayers(true, inputSize, hiddenSize, outputSize, rules);

            using (var nn = ctx.NeuralNetworkFactory.CreateMultilayerPerceptron(layers, new MultilayerPerceptronProperties { GradientComputationMethod = GradientComputationMethod.FeedForward }))
            using (var batch = new SupervisedBatch())
            using (var errors = ctx.DataArrayFactory.Create(maxIterations))
            {
                for (int i = 0; i < trainingData.GetLength(0); i++)
                {
                    batch.Add(
                        ctx.DataArrayFactory.Create(new[] { NNTestHelpers.Normalize(trainingData[i, 0], minInput, maxInput) }),
                        ctx.DataArrayFactory.Create(new[] { NNTestHelpers.Normalize(trainingData[i, 1], minOutput, maxOutput) }),
                        ctx.DataArrayFactory.Create(1));
                }

                bool first = true;
                var sw = new Stopwatch();
                for (int it = 0; it < maxIterations; it++)
                {
                    nn.Train(batch);

                    if (first)
                    {
                        using (var weights = ctx.DataArrayFactory.Create(nn.NumberOfWeights))
                        {
                            nn.GetWeights(weights);
                            float[] wa = new float[weights.Size];
                            await weights.Read(wa);

                            // It must be randomized:
                            Assert.IsTrue(wa.Sum() != 0.0f);
                        }
                        first = false;
                        sw.Start();
                    }

                    ctx.VectorUtils.CalculateMSE(batch, errors, it);
                }

                float[] mses = new float[maxIterations];
                await errors.Read(mses);

                sw.Stop();

                Console.WriteLine("Ellapsed: {0} sec", sw.Elapsed.TotalSeconds);
                foreach (var mse in mses) Console.WriteLine("Error: {0}", mse.ToString("0.00000000"));
            }
        }

        #endregion
    }
}
