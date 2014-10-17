using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using NeoComp.Adjustables;

namespace NeoComp.Networks.Neural
{
    public interface IActivationNeuron : INeuron, IAdjustableItem
    {
        double Bias { get; set; }

        IActivationFunction ActivationFunction { get; }
    }
}
