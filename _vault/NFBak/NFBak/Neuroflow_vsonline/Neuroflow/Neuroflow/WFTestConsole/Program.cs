using System;
using System.Linq;
using System.Activities;
using System.Activities.Statements;
using Neuroflow.Networks.Neural;
using Neuroflow.Native;
using System.Diagnostics;
using Neuroflow.Networks.Neural.Managed;
using Neuroflow.Networks.Neural.Learning;
namespace WFTestConsole
{

    class Program
    {
        private static void Invoke()
        {
            WorkflowInvoker.Invoke(new ImgNoiseTest());
            //WorkflowInvoker.Invoke(new PussyDetectorTest());

            //DoTest(new ManagedNNInitParameters(), false);
            //DoTest(new NativeNNInitParameters(), false);
            //DoTest(new ManagedNNInitParameters(), true);
            //DoTest(new NativeNNInitParameters(), true);
        }

        private static void DoTest(NNInitParameters initPars, bool parallelize)
        {
            Console.WriteLine("Testing: {0}", initPars.GetType().Name + (parallelize ? " parallel" : " no parallel"));

            int count = 10000;

            var sw = new Stopwatch();

            var arch = CreateArch(initPars);

            using (var nn = arch.CreateNetwork())
            using (var vectorBuff = nn.CreateVectorBuffer())
            {
                int cpus = Environment.ProcessorCount * 2;
                var input = new float[10];
                var output = Enumerable.Repeat(1.1f, 20).ToArray();

                if (parallelize)
                {
                    var contexts = new NeuralComputationContext[cpus];
                    for (int i = 0; i < cpus; i++) contexts[i] = nn.CreateContext();
                    sw.Start();
                    for (int i = 0; i < count; i+= cpus)
                    {
                        int ccount = cpus;
                        if (i + ccount > count) ccount = (i + ccount) - count;
                        System.Threading.Tasks.Parallel.For(0, ccount, x =>
                        {
                            var ctx = contexts[x];
                            nn.WriteInput(ctx, vectorBuff.GetOrCreate(x, 0, () => input));
                            nn.Reset(ctx);
                            nn.Iteration(ctx);
                            nn.Backpropagate(ctx, BackprogrationMode.FeedForward);
                            nn.ReadOutput(ctx, output);
                        });
                    }
                    sw.Stop();
                }
                else
                {
                    using (var ctx = nn.CreateContext())
                    {
                        sw.Start();
                        for (int i = 0; i < count; i++)
                        {
                            nn.WriteInput(ctx, vectorBuff.GetOrCreate(i % cpus, 0, () => input));
                            nn.Reset(ctx);
                            nn.Iteration(ctx);
                            nn.Backpropagate(ctx, BackprogrationMode.FeedForward);
                            nn.ReadOutput(ctx, output);
                        }
                        sw.Stop();
                    }
                }
            }

            Console.WriteLine(sw.Elapsed.TotalMilliseconds);
        }

        private static NeuralNetworkArchitecture CreateArch(NNInitParameters initPars)
        {
            var arch = new StandardMultilayerArchitecture
            {
                InitParameters = initPars,
                InputSize = 10,
                HiddenLayers = new[] 
                { 
                    new ActivationLayer(200, new SigmoidActivationFunction(), new GradientDescentRule()),
                    new ActivationLayer(100, new SigmoidActivationFunction(), new GradientDescentRule()) 
                },
                OutputLayer = new ActivationLayer(20, new LinearActivationFunction(), new GradientDescentRule())
            };

            return arch;
        }

        static void Main(string[] args)
        {
            try
            {
                Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            PressAny();
        }

        private static void PressAny()
        {
            Console.WriteLine();
            Console.WriteLine("Press any ...");
            Console.ReadKey();
        }
    }
}
