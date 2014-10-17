using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks.Neural
{
    public interface IActivationFunction
    {
        double Function(double value);
    }

    public abstract class ActivationFunction : IActivationFunction
    {
        public abstract double Function(double value);
    }
}
