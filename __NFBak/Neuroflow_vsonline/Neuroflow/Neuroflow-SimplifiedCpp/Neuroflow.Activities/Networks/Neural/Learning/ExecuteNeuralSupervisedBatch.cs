using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Neuroflow.Core.Vectors;
using Neuroflow.Networks.Neural.Learning;
using Neuroflow.Networks.Neural;

namespace Neuroflow.Activities.Networks.Neural.Learning
{
    internal sealed class ExecuteNeuralSupervisedBatch : AsyncCodeActivity<BatchExecutionResult>
    {
        public NNAlgorithm Algorithm { get; set; }

        [RequiredArgument]
        public InArgument<NeuralNetwork> Network { get; set; }

        [RequiredArgument]
        public InArgument<NeuralVectorFlowBatch> Batch { get; set; }

        [RequiredArgument]
        public InArgument<NeuralComputationContext> NCContext { get; set; }

        [RequiredArgument]
        public InArgument<VectorBuffer<float>> VectorBuffer { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            var network = Network.Get(context);
            var batch = Batch.Get(context);
            var ctx = NCContext.Get(context);
            var vbuff = VectorBuffer.Get(context);

            NeuralSupervisedBatchExecution exec;
            switch (Algorithm)
            {
                case NNAlgorithm.StreamedTraining:
                    exec = network.GetAlgorithm<StreamedTraining>();
                    break;
                case NNAlgorithm.UnorderedTraining:
                    exec = network.GetAlgorithm<UnorderedTraining>();
                    break;
                case NNAlgorithm.Validation:
                    exec = network.GetAlgorithm<Validation>();
                    break;
                default:
                    throw new InvalidOperationException("Algorithm " + Algorithm + " is unknown.");
            }

            Func<NeuralSupervisedBatchExecution, NeuralComputationContext, VectorBuffer<float>, NeuralVectorFlowBatch, BatchExecutionResult> work = ExecuteAndGetResult;
            context.UserState = work;
            return work.BeginInvoke(exec, ctx, vbuff, batch, callback, state);
        }

        private BatchExecutionResult ExecuteAndGetResult(NeuralSupervisedBatchExecution exec, NeuralComputationContext context, VectorBuffer<float> vectorBuffer, NeuralVectorFlowBatch batch)
        {
            exec.ExcuteBatch(context, vectorBuffer, batch);
            float[] errors = new float[exec.Network.OutputInterfaceLength];
            exec.ReadError(context, errors);
            var result = new BatchExecutionResult(errors, errors.Average());
            return result;
        }

        protected override BatchExecutionResult EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            return ((Func<NeuralSupervisedBatchExecution, NeuralComputationContext, VectorBuffer<float>, NeuralVectorFlowBatch, BatchExecutionResult>)context.UserState).EndInvoke(result);
        }
    }
}
