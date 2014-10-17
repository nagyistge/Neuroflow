using System;
using System.Linq;
using System.Activities;
using System.Activities.Statements;
using Neuroflow.Networks.Neural;

namespace Neuroflow.WFTestConsole
{

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                WorkflowInvoker.Invoke(new ImgNoiseTest());
                //WorkflowInvoker.Invoke(new PussyDetectorTest());
                //WorkflowInvoker.Invoke(new LeCunTest());
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:");
                Console.WriteLine(ex);
            }

            Console.WriteLine();
            Console.WriteLine("Press any ...");
            Console.ReadKey();
        }
    }
}
