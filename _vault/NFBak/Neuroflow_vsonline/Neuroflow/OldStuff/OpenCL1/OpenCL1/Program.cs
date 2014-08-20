using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCLTemplate;
using System.Diagnostics;
using Cloo;

namespace OpenCL1
{
    class Program
    {
        static string vecSum = @"
                     __kernel void
                    floatVectorSum(__global       float * v1,
                                   __global       float * v2)
                    {
                        // Vector element index
                        int i = get_global_id(0);
                        v1[i] = (v1[i] * v2[i]) + (v1[i] * v2[i]) + (v1[i] * v2[i]) + (v1[i] * v2[i]) + (v1[i] * v2[i]) + (v1[i] * v2[i]) + (v1[i] * v2[i]) + (v1[i] * v2[i]);
                    }";

        //We want to sum 20000 numbers
        static int n = 200000, count = 10000;

        static Stopwatch sw = new Stopwatch();
        
        static void Main()
        {
            //Initializes OpenCL Platforms and Devices and sets everything up
            CLCalc.InitCL();

            //Create vectors with 2000 numbers
            float[] v1 = new float[n], v2 = new float[n];

            var vResult = new float[n];

            //Creates population for v1 and v2
            for (int i = 0; i < n; i++)
            {
                v1[i] = (float)i / 10;
                v2[i] = -(float)i / 9;
            }

            //var prog = new ComputeProgram(CLCalc.Program.Context, "");

            //Compiles the source codes. The source is a string array because the user may want
            //to split the source into many strings.
            CLCalc.Program.Compile(new string[] { vecSum });

            //Gets host access to the OpenCL floatVectorSum kernel
            CLCalc.Program.Kernel VectorSum = new CLCalc.Program.Kernel("floatVectorSum");

            //Creates vectors v1 and v2 in the device memory
            CLCalc.Program.Variable varV1 = new CLCalc.Program.Variable(v1);
            CLCalc.Program.Variable varV2 = new CLCalc.Program.Variable(v2);

            //Arguments of VectorSum kernel
            CLCalc.Program.Variable[] args = new CLCalc.Program.Variable[] { varV1, varV2 };

            //How many workers will there be? We need "n", one for each element
            int[] workers = new int[1] { n };

            sw.Start();
            //Execute the kernel
            for (int i = 0; i < count; i++) DoOCL(VectorSum, args, workers);
            sw.Stop();

            //Read device memory varV1 to host memory vResult
            varV1.ReadFromDeviceTo(vResult);

            Console.WriteLine("OpenCL: {0}", sw.ElapsedTicks);

            sw.Restart();
            for (int i = 0; i < count; i++) DoCPU(v1, v2, vResult);
            sw.Stop();

            Console.WriteLine("CPU: {0}", sw.ElapsedTicks);

            PressAny();
        }

        private static void DoCPU(float[] v1, float[] v2, float[] vResult)
        {
            for (int i = 0; i < n; i++) vResult[i] = (v1[i] * v2[i]) + (v1[i] * v2[i]) + (v1[i] * v2[i]) + (v1[i] * v2[i]) + (v1[i] * v2[i]) + (v1[i] * v2[i]) + (v1[i] * v2[i]) + (v1[i] * v2[i]);
        }

        private static void DoOCL(CLCalc.Program.Kernel VectorSum, CLCalc.Program.Variable[] args, int[] workers)
        {
            VectorSum.Execute(args, workers);
        }

        private static void PressAny()
        {
            Console.WriteLine();
            Console.WriteLine("Press any ...");
            Console.ReadKey();
        }
    }
}
