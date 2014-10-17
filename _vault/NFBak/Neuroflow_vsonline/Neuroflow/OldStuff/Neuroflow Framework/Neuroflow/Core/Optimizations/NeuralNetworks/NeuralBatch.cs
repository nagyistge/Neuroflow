using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;
using Neuroflow.Core.Optimizations;

namespace Neuroflow.Core.Optimizations.NeuralNetworks
{
    public enum TrainingResetSchedule { None, AfterExecution, BeforeExecution }
    
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow, Name = "neuralBatch")]
    public sealed class NeuralBatch : VectorFlowBatch<double>
    {
        public NeuralBatch(VectorFlow<double> vectorFlow)
            : base(vectorFlow)
        {
            Contract.Requires(vectorFlow != null);
        }

        public NeuralBatch(IList<VectorFlow<double>> vectorFlowList)
            : base(vectorFlowList)
        {
            Contract.Requires(vectorFlowList != null && vectorFlowList.Count > 0);
        }

        public NeuralBatch(IEnumerable<VectorFlow<double>> vectorFlowCollection)
            : base(vectorFlowCollection)
        {
            Contract.Requires(vectorFlowCollection != null);
        }

        [DataMember]
        public TrainingResetSchedule ResetSchedule { get; set; }
    }
}
