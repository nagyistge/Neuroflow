using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core.Computations;

namespace Neuroflow.Core.NeuralNetworks.Learning
{
    public sealed class BackwardValue
    {
        public double Error { get; private set; }

        public double Gradient { get; private set; }
        
        public override string ToString()
        {
            return string.Format("E: {0}, G: {1}", Error, Gradient);
        }

        internal void Add(double error, double gradient)
        {
            Error = error;
            Gradient += gradient;
        }

        internal void Set(double error, double gradient)
        {
            Error = error;
            Gradient = gradient;
        }

        internal void Reset()
        {
            Error = 0;
            Gradient = 0;
        }
    }
}
