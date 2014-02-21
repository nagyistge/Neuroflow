using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class BackwardValue
    {
        double error, gradient, errorSum;

        public double Error
        {
            get { return error; }
        }

        public double ErrorSum
        {
            get { return errorSum; }
        }

        public double Gradient
        {
            get { return gradient; }
        }
        
        public override string ToString()
        {
            return string.Format("E: {0}, G: {1}", error, gradient);
        }

        internal void Add(double error, double input)
        {
            this.error = error;
            this.errorSum += error;
            this.gradient += error * input;
        }

        internal void Set(double error, double input)
        {
            this.error = error;
            this.errorSum = error;
            this.gradient = error * input;
        }
    }
}
