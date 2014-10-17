using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks.Neural
{
    public interface IDerivatableActivationFunction : IActivationFunction
    {
        double Derivate(double value);
    }
}
