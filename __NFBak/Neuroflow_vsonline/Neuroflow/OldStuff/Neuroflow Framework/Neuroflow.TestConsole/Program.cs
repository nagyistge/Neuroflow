using System;
using System.Linq;
using System.Activities;
using System.Activities.Statements;

namespace Neuroflow.TestConsole
{

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //WorkflowInvoker.Invoke(new PussyDetectorTest());
                WorkflowInvoker.Invoke(new ImgNoiseTest());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.WriteLine("Press any key, baszod ...");
            Console.ReadKey();
        }
    }
}
