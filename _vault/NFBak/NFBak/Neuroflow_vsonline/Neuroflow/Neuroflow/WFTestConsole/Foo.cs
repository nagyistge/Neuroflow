using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFTestConsole
{
    [ToolboxItem("Foo")]
    public class Foo : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            Debug.WriteLine("Foo");
        }
    }
}
