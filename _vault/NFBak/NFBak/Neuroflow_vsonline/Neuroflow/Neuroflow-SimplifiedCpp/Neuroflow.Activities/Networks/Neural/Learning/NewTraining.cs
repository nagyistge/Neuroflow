using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Networks.Neural.Learning;

namespace Neuroflow.Activities.Networks.Neural.Learning
{
    public abstract class NewTraining<T> : NewObjectActivity<Training>
        where T : Training
    {
        protected override Type ObjectType
        {
            get { return typeof(T); }
        }
    }
}
