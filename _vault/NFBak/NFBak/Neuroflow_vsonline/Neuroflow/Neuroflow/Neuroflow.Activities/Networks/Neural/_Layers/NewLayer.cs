using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Networks.Neural;

namespace Neuroflow.Activities.Networks.Neural
{
    public abstract class NewLayer<T> : NewObjectActivity<Layer>
        where T : Layer
    {
        protected override Type ObjectType
        {
            get { return typeof(T); }
        }
    }
}
