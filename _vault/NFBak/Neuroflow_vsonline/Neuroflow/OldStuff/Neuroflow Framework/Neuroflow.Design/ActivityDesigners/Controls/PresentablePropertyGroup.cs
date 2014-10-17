using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.Model;

namespace Neuroflow.Design.ActivityDesigners.Controls
{
    public sealed class PresentablePropertyGroup
    {
        public string Name { get; set; }

        public ModelItem ModelItem { get; set; }

        public IEnumerable<string> PropertyNames { get; set; }
    }
}
