using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.OpenCL;
using System.IO;
using Neuroflow.Networks.Neural;
using Neuroflow.Networks.Neural.Learning;
using Neuroflow.Networks.Neural.CPU;
using Cloo;
using Neuroflow.Networks.Neural.OpenCL;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TestNN();
                //TestNet();
                //TestOCL();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            PressAny();
        }

        private static void TestNN()
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
                float[] inputs = new float[10];
                float[] outputs = new float[2];

                nn.WriteInput(inputs);

                nn.ReadOutput(outputs);
            }
        }

        private static void TestNet()
        {
            var initRule = new NoisedWeightInitializationRule();
            var gdRule = new GradientDescentRule();

            var input = new InputLayer(6).Connectable();
            var mid1 = new ActivationLayer(9, new SigmoidActivationFunction(), initRule, gdRule).Connectable();
            var mid2 = new ActivationLayer(11, new SigmoidActivationFunction(), initRule, gdRule).Connectable();
            var mid3 = new ActivationLayer(5, new SigmoidActivationFunction(), initRule, gdRule).Connectable();
            var output = new ActivationLayer(2, new LinearActivationFunction(), initRule, gdRule).Connectable();

            input.LowerLayers.Add(mid1);
            input.LowerLayers.Add(mid2);
            mid1.LowerLayers.Add(output);
            mid3.LowerLayers.Add(output);
            mid2.LowerLayers.Add(mid3);

            var layers = new[] { input, mid1, mid2, mid3, output };

            var groups = new GroupedLayers(layers);

            Console.WriteLine(string.Join(" - ", from g in groups.LayerGroups
                                               select string.Join(",", g.Select(l => l.Size))));

            var cpuNN = new CPUNeuralNetwork(layers);

            cpuNN.Iteration();
        }

        private static void TestOCL()
        {
            float[] inputs = { 0.1234f, 0.211f, 0.3f, 0.45f, 0.543f };
            float[] weights = { 0.6f, 0.8f, 0.5f, 0.3f, 0.1f, 0.5f, 0.3f, 0.5f, 0.5f, 0.7f, 0.3f, 0.3f, 0.11f, 0.9f, 0.5f };
            float alpha = 1.05f;
            float[] outputs = new float[3];

            ComputeCPU(inputs, weights, outputs, alpha);

            ShowOutput("CPU", outputs);

            outputs = new float[3];

            ComputeOpenCL(inputs, weights, outputs, alpha);

            ShowOutput("OpenCL", outputs);
        }

        private static void ComputeOpenCL(float[] inputs, float[] weights, float[] outputs, float alpha)
        {
            string program = File.ReadAllText("code.cl");
            
            using (var ctx = new OpenCLContext())
            using (var prog = ctx.CompileProgram(program))
            using (var kernel = prog.CreateKernel("ActivationNeuron"))
            using (var bInputs = ctx.CreateBuffer(inputs, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer))
            using (var bWeights = ctx.CreateBuffer(weights, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer))
            using (var bAlpha = ctx.CreateBuffer(new[] { alpha }, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer))
            using (var bInputCount = ctx.CreateBuffer(new[] { inputs.Length }, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer))
            using (var bOutputs = ctx.CreateBuffer(outputs, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer))
            using (var queue = ctx.CreateQueue())
            {
                kernel.SetArguments(bInputs, bWeights, bOutputs, bInputCount, bAlpha);
                queue.Execute(kernel, new long[] { outputs.Length });
                queue.ReadAll(bOutputs, outputs);
            }
        }

        private static void ShowOutput(string title, float[] outputs)
        {
            Console.WriteLine(title);
            Console.WriteLine(string.Join(", ", outputs));
        }

        private static void ComputeCPU(float[] inputs, float[] weights, float[] outputs, float alpha)
        {
            for (int outputIndex = 0; outputIndex < outputs.Length; outputIndex++)
            {
                float sum = 0.0f;
                for (int inputIndex = 0; inputIndex < inputs.Length; inputIndex++)
                {
                    sum += inputs[inputIndex] * weights[inputs.Length * outputIndex + inputIndex];
                }
                outputs[outputIndex] = (2.0f / (1.0f + (float)Math.Exp(-alpha * sum))) - 1.0f;
            }
        }

        private static void PressAny()
        {
            Console.WriteLine();
            Console.WriteLine("Press any ...");
            Console.ReadKey();
        }
    }
}
