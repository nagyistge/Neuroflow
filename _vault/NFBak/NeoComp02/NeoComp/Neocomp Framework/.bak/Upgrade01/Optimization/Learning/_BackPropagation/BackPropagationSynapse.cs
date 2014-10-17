using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public class BackPropagationSynapse : Synapse, IBackPropagationSynapse
    {
        public BackPropagationSynapse()
            : base()
        {
        }

        public BackPropagationSynapse(double weight)
            : base(weight)
        {
        }

        public double Error { get; internal set; }

        public double Delta { get; private set; }

        double IDeltaBasedAdjustable.Delta
        {
            get { return Delta; }
            set { Delta = value; }
        }

        double IBackPropagationSynapse.Error
        {
            get { return Error; }
            set { Error = value; }
        }
    }
}
