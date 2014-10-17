using System;
using System.Linq;
using System.Activities;
using System.Activities.Statements;

namespace WFTestConsole
{

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var wf = new Workflow1();
                WorkflowInvoker.Invoke(wf);
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadKey();
        }
    }
}
