using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Vectors;
using Neuroflow.Core.Serialization;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural
{
    public enum ResetSchedule { None, AfterExecution, BeforeExecution }

    [Serializable, Known, DataContract(Namespace = xmlns.Neuroflow)]
    public sealed class NeuralVectorFlowBatch : VectorFlowBatch<float>
    {
        public NeuralVectorFlowBatch(NeuralVectorFlow vectorFlow, ResetSchedule resetSchedule = ResetSchedule.None)
            : base(vectorFlow)
        {
            Contract.Requires(vectorFlow != null);

            ResetSchedule = resetSchedule;
        }

        public NeuralVectorFlowBatch(NeuralVectorFlow[] vectorFlows, ResetSchedule resetSchedule = ResetSchedule.None)
            : base(vectorFlows)
        {
            Contract.Requires(vectorFlows != null);
            Contract.Requires(vectorFlows.Length > 0);

            ResetSchedule = resetSchedule;
        }

        [DataMember]
        public ResetSchedule ResetSchedule { get; private set; }
    }
}
