using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationAPI;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Networks.Neural
{
    public abstract class ActivationFunction
    {
        public ActivationFunction()
            : this(1.0)
        {
        }
        
        public ActivationFunction(double alpha)
        {
            Contract.Requires(alpha > 0.0);

            Alpha = alpha;
        }

        [InitValue(1.0)]
        [DefaultValue(1.0)]
        [Category(PropertyCategories.Math)]
        public double Alpha { get; set; }

        public abstract string Function(ComputationBlock block, string value);

        public abstract string Derivate(ComputationBlock block, string value);
    }
}
