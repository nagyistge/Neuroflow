using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.ComponentModel;
using Neuroflow.Core.ComponentModel;

namespace Neuroflow.Networks.Neural
{
    public abstract class NNInitParameters
    {
        public abstract Type NeuralNetworkType { [Pure] get; }

        [DisplayName("Recurrent Options")]
        [Category(PropertyCategories.Structure)]
        public RecurrentOptions RecurrentOptions { get; set; }
    }

    public abstract class NNInitParameters<T> : NNInitParameters
        where T : NeuralNetwork
    {
        public sealed override Type NeuralNetworkType
        {
            [Pure]
            get { return typeof(T); }
        }
    }
}
