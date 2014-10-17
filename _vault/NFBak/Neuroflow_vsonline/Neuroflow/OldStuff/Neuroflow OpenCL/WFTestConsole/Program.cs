using System;
using System.Linq;
using System.Activities;
using System.Activities.Statements;

namespace WFTestConsole
{

    class Program
    {
        private static void Invoke()
        {
            WorkflowInvoker.Invoke(new ImgNoiseTest());
            //WorkflowInvoker.Invoke(new PussyDetectorTest());
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
