﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.NeuralNetworks.Learning
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow, Name = "neuralValidation")]
    public sealed class Validation : NeuralBatchExecution
    {
        public Validation(NeuralNetwork network)
            : base(network)
        {
            Contract.Requires(network != null);
        }
    }
}
