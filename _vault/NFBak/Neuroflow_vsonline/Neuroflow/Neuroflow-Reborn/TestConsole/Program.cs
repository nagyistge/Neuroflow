using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neuroflow;
using Neuroflow.Data;
using Neuroflow.NeuralNetworks;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //new Neuroflow.UT.MultilayerPerceptronRecurrentTests().OCLMLPTrainBPTTOfflineCPUTest().Wait();

                //Console.WriteLine("Managed");
                //ManagedMLPTrainBPOnlineCPUTest().Wait();
                //Console.WriteLine();

                Console.WriteLine("OpenCL CPU");
                OCLMLPTrainBPOnlineCPUTest().Wait();
                Console.WriteLine();

                Console.WriteLine("OpenCL GPU");
                OCLMLPTrainBPOnlineGPUTest().Wait();
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            //Console.WriteLine("\nPress any ...");
            //Console.ReadKey();
        }

        #region Training

        static async Task ManagedMLPTrainBPOnlineCPUTest()
        {
            using (var ctx = new ManagedContext())
            {
                await MLPTrainBPOnlineTest(ctx);
            }
        }

        static async Task OCLMLPTrainBPOnlineCPUTest()
        {
            using (var ctx = new OCLContext("CPU"))
            //using (var ctx = new OCLContext("(sse2,avx)"))
            {
                await MLPTrainBPOnlineTest(ctx);
            }
        }

        static async Task OCLMLPTrainBPOnlineGPUTest()
        {
            //using (var ctx = new OCLContext("HD"))
            using (var ctx = new OCLContext("gpu"))
            {
                await MLPTrainBPOnlineTest(ctx);
            }
        }

        static async Task MLPTrainBPOnlineTest(ComputationContext ctx)
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
            const int hidden1Size = 512;
            const int hidden2Size = 256;
            const int outputSize = 1;

            const int maxIterations = 1000;

            var init = new UniformRandomizeWeights(.3f);

            var algo = new GradientDescentLearningRule
            {
                LearningRate = 0.01f,
                Momentum = 0.25f,
                WeightUpdateMode = WeigthUpdateMode.Online,
                Smoothing = false
            };

            //var algo = new CrossEntropyLearningRule
            //{
            //    NarrowingRate = 0.85f,
            //    MutationChance = 0.001f,
            //    MeanMutationStrength = 0.05f,
            //    StdDevMutationStrength = 1.0f,
            //    PopulationSize = 10
            //};

            //var algo = new AlopexBLearningRule();
            //algo.StepSizeB = 0.001f;
            //algo.StepSizeA = 0.0001f;
            //algo.ForgettingRate = 0.35f;

            var layers = new[]
            {
                new Layer(inputSize),
                new Layer(hidden1Size)
                {
                    Behaviors =
                    {
                        init,
                        algo
                    },
                    Descriptions =
                    {
                        new ActivationDescription(ActivationFunction.Sigmoid)
                    }
                },
                new Layer(hidden2Size)
                {
                    Behaviors =
                    {
                        init,
                        algo
                    },
                    Descriptions =
                    {
                        new ActivationDescription(ActivationFunction.Sigmoid)
                    }
                },
                new Layer(outputSize)
                {
                    Behaviors =
                    {
                        init,
                        algo
                    },
                    Descriptions =
                    {
                        new ActivationDescription(ActivationFunction.Linear)
                    }
                },
            };

            layers[0].OutputConnections.AddOneWay(layers[1]);
            layers[1].OutputConnections.AddOneWay(layers[2]);
            layers[2].OutputConnections.AddOneWay(layers[3]);

            using (var nn = ctx.NeuralNetworkFactory.CreateMultilayerPerceptron(layers, new MultilayerPerceptronProperties { GradientComputationMethod = GradientComputationMethod.FeedForward }))
            using (var batch = new SupervisedBatch())
            using (var errors = ctx.DataArrayFactory.Create(maxIterations))
            {
                for (int i = 0; i < trainingData.GetLength(0); i++)
                {
                    batch.Add(
                        ctx.DataArrayFactory.Create(new[] { Normalize(trainingData[i, 0], minInput, maxInput) }),
                        ctx.DataArrayFactory.Create(new[] { Normalize(trainingData[i, 1], minOutput, maxOutput) }),
                        ctx.DataArrayFactory.Create(1));
                }

                bool first = true;
                var sw = new Stopwatch();
                for (int it = 0; it < maxIterations; it++)
                {
                    nn.Train(batch);

                    ctx.VectorUtils.CalculateMSE(batch, errors, it);

                    if (first)
                    {
                        sw.Start();
                        first = false;
                    }
                }

                float[] mses = new float[maxIterations];
                await errors.Read(mses);

                sw.Stop();

                //foreach (var mse in mses) Console.WriteLine("Error: {0}", mse.ToString("0.00000000"));

                Console.WriteLine("MSE: {0}", mses.Last());

                Console.WriteLine("Ellapsed: {0} ms", sw.Elapsed.TotalMilliseconds);
            }
        }

        private static float Normalize(float value, float min, float max)
        {
            return ((value - min) / (max - min)) * 2.0f - 1.0f;
        }

        #endregion
    }
}
