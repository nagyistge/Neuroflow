using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuroflow.Data;
using Neuroflow.NeuralNetworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.UT
{
    [TestClass]
    public class MultilayerPerceptronRecurrentTests
    {
        #region R AB

        [TestMethod]
        [TestCategory(TestCategories.Managed)]
        [TestCategory(TestCategories.Global)]
        [TestCategory(TestCategories.AlopexB)]
        public async Task ManagedMLPTrainGlobalRecABTest()
        {
            using (var ctx = new ManagedContext())
            {
                await MLPTrainRecTest(ctx, GradientComputationMethod.None, GetABRules(0.01f, 0.01f, 0.95f));
            }
        }

        #endregion

        #region R CE

        [TestMethod]
        [TestCategory(TestCategories.Managed)]
        [TestCategory(TestCategories.Global)]
        [TestCategory(TestCategories.CrossEntropy)]
        public async Task ManagedMLPTrainGlobalRecCETest()
        {
            using (var ctx = new ManagedContext())
            {
                await MLPTrainRecTest(ctx, GradientComputationMethod.None, GetCERules(0.025f, 0.05f, 0.5f, 10));
            }
        }

        #endregion

        #region BPTT GD

        [TestMethod]
        [TestCategory(TestCategories.Managed)]
        [TestCategory(TestCategories.BPTT)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task ManagedMLPTrainBPTTGDOnlineTest()
        {
            using (var ctx = new ManagedContext())
            {
                await MLPTrainRecTest(ctx, GradientComputationMethod.BPTT, GetGDRules(WeigthUpdateMode.Online, 0.01f));
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.Managed)]
        [TestCategory(TestCategories.BPTT)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task ManagedMLPTrainBPTTGDOfflineTest()
        {
            using (var ctx = new ManagedContext())
            {
                await MLPTrainRecTest(ctx, GradientComputationMethod.BPTT, GetGDRules(WeigthUpdateMode.Offline, 0.01f));
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        [TestCategory(TestCategories.BPTT)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task OCLMLPTrainBPTTGDOnlineCPUTest()
        {
            using (var ctx = new OCLContext("cpu"))
            {
                await MLPTrainRecTest(ctx, GradientComputationMethod.BPTT, GetGDRules(WeigthUpdateMode.Online, 0.01f));
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        [TestCategory(TestCategories.BPTT)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task OCLMLPTrainBPTTGDOfflineCPUTest()
        {
            using (var ctx = new OCLContext("cpu"))
            {
                await MLPTrainRecTest(ctx, GradientComputationMethod.BPTT, GetGDRules(WeigthUpdateMode.Offline, 0.01f));
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        [TestCategory(TestCategories.BPTT)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task OCLMLPTrainBPTTGDOnlineGPUTest()
        {
            using (var ctx = new OCLContext("gpu"))
            {
                await MLPTrainRecTest(ctx, GradientComputationMethod.BPTT, GetGDRules(WeigthUpdateMode.Online, 0.01f));
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        [TestCategory(TestCategories.BPTT)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task OCLMLPTrainBPTTGDOfflineGPUTest()
        {
            using (var ctx = new OCLContext("gpu"))
            {
                await MLPTrainRecTest(ctx, GradientComputationMethod.BPTT, GetGDRules(WeigthUpdateMode.Offline, 0.01f));
            }
        } 

        #endregion

        #region RTLR

        [TestMethod]
        [TestCategory(TestCategories.Managed)]
        [TestCategory(TestCategories.RTLR)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task ManagedMLPTrainRTLRGDOnlineTest()
        {
            using (var ctx = new ManagedContext())
            {
                await MLPTrainRecTest(ctx, GradientComputationMethod.RTLR, GetGDRules(WeigthUpdateMode.Online, 0.01f));
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.Managed)]
        [TestCategory(TestCategories.RTLR)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task ManagedMLPTrainRTLRGDOfflineTest()
        {
            using (var ctx = new ManagedContext())
            {
                await MLPTrainRecTest(ctx, GradientComputationMethod.RTLR, GetGDRules(WeigthUpdateMode.Offline, 0.01f));
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        [TestCategory(TestCategories.RTLR)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task OCLMLPTrainRTLRGDOnlineCPUTest()
        {
            try
            {
                using (var ctx = new OCLContext("cpu"))
                //using (var ctx = new OCLContext("avx"))
                {
                    await MLPTrainRecTest(ctx, GradientComputationMethod.RTLR, GetGDRules(WeigthUpdateMode.Online, 0.01f));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        [TestCategory(TestCategories.RTLR)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task OCLMLPTrainRTLRGDOfflineCPUTest()
        {
            using (var ctx = new OCLContext("cpu"))
            {
                await MLPTrainRecTest(ctx, GradientComputationMethod.RTLR, GetGDRules(WeigthUpdateMode.Offline, 0.01f));
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        [TestCategory(TestCategories.RTLR)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task OCLMLPTrainRTLRGDOnlineGPUTest()
        {
            using (var ctx = new OCLContext("gpu"))
            {
                await MLPTrainRecTest(ctx, GradientComputationMethod.RTLR, GetGDRules(WeigthUpdateMode.Online, 0.01f));
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.OCL)]
        [TestCategory(TestCategories.RTLR)]
        [TestCategory(TestCategories.GradientDescent)]
        public async Task OCLMLPTrainRTLRGDOfflineGPUTest()
        {
            using (var ctx = new OCLContext("gpu"))
            {
                await MLPTrainRecTest(ctx, GradientComputationMethod.RTLR, GetGDRules(WeigthUpdateMode.Offline, 0.01f));
            }
        }

        #endregion

        private LayerBehavior[] GetCERules(float mutChance, float meanMut, float stdMut, int popSize = 10)
        {
            var algo = new CrossEntropyLearningRule
            {
                MutationChance = mutChance,
                MeanMutationStrength = meanMut,
                StdDevMutationStrength = stdMut,
                PopulationSize = popSize
            };

            return new LayerBehavior[] { algo };
        }

        private LayerBehavior[] GetABRules(float rate1, float rate2, float rate3)
        {
            var init = new UniformRandomizeWeights(0.3f);

            var algo = new AlopexBLearningRule
            {
                StepSizeB = rate1,
                StepSizeA = rate2,
                ForgettingRate = rate3
            };

            return new LayerBehavior[] { init, algo };
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

        private async Task MLPTrainRecTest(ComputationContext ctx, GradientComputationMethod method, params LayerBehavior[] rules)
        {
            var trainingData =
                new[]
                {
                    new[]
                    {
                        Tuple.Create( -1.0f, (float[])null),
                        Tuple.Create( -1.0f, (float[])null),
                        Tuple.Create( -1.0f, new[] { -1.0f, -1.0f, -1.0f }),
                    },
                    new[]
                    {
                        Tuple.Create( -1.0f, (float[])null),
                        Tuple.Create( -1.0f, (float[])null),
                        Tuple.Create( 1.0f, new[] { -1.0f, -1.0f, 1.0f }),
                    },
                    new[]
                    {
                        Tuple.Create( -1.0f, (float[])null),
                        Tuple.Create( 1.0f, (float[])null),
                        Tuple.Create( -1.0f, new[] { -1.0f, 1.0f, -1.0f }),
                    },
                    new[]
                    {
                        Tuple.Create( -1.0f, (float[])null),
                        Tuple.Create( 1.0f, (float[])null),
                        Tuple.Create( 1.0f, new[] { -1.0f, 1.0f, 1.0f }),
                    },
                    new[]
                    {
                        Tuple.Create( 1.0f, (float[])null),
                        Tuple.Create( -1.0f, (float[])null),
                        Tuple.Create( -1.0f, new[] { 1.0f, -1.0f, -1.0f }),
                    },
                    new[]
                    {
                        Tuple.Create( 1.0f, (float[])null),
                        Tuple.Create( -1.0f, (float[])null),
                        Tuple.Create( 1.0f, new[] { 1.0f, -1.0f, 1.0f }),
                    },
                    new[]
                    {
                        Tuple.Create( 1.0f, (float[])null),
                        Tuple.Create( 1.0f, (float[])null),
                        Tuple.Create( -1.0f, new[] { 1.0f, 1.0f, -1.0f }),
                    },
                    new[]
                    {
                        Tuple.Create( 1.0f, (float[])null),
                        Tuple.Create( 1.0f, (float[])null),
                        Tuple.Create( 1.0f, new[] { 1.0f, 1.0f, 1.0f }),
                    }
                };

            int inputSize = 1;
            int hiddenSize = method == GradientComputationMethod.RTLR ? 64 : 8;
            int outputSize = 3;

            int maxIterations = method == GradientComputationMethod.RTLR ? 30 : 1000;

            var layers = NNTestHelpers.CreateGDMLPLayers(false, inputSize, hiddenSize, outputSize, rules);

            using (var nn = ctx.NeuralNetworkFactory.CreateMultilayerPerceptron(layers, new MultilayerPerceptronProperties { GradientComputationMethod = method }))
            using (var batch = new SupervisedBatch())
            using (var errors = ctx.DataArrayFactory.Create(maxIterations))
            {
                foreach (var dataEntry in trainingData)
                {
                    var sample = new SupervisedSample();

                    foreach (var sampleEntry in dataEntry)
                    {
                        if (sampleEntry.Item2 == null)
                        {
                            sample.Add(ctx.DataArrayFactory.CreateConst(new[] { sampleEntry.Item1 }));
                        }
                        else
                        {
                            sample.Add(
                                ctx.DataArrayFactory.CreateConst(new[] { sampleEntry.Item1 }),
                                ctx.DataArrayFactory.CreateConst(sampleEntry.Item2),
                                ctx.DataArrayFactory.Create(sampleEntry.Item2.Length));
                        }
                    }

                    batch.Add(sample);
                }

                bool first = true;
                var sw = new Stopwatch();
                for (int it = 0; it < maxIterations; it++)
                {
                    nn.Train(batch);

                    await ctx.Finish();

                    ctx.VectorUtils.CalculateMSE(batch, errors, it);

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
                }

                float[] mses = new float[maxIterations];
                await errors.Read(mses);

                sw.Stop();

                foreach (var mse in mses) Console.WriteLine("Error: {0}", mse.ToString("0.00000000"));

                Console.WriteLine("Ellapsed: {0} ms", sw.Elapsed.TotalMilliseconds);
            }
        }
    }
}
