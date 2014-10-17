using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Neuroflow.Core.Vectors;
using System.Activities.Presentation;
using Neuroflow.Core.ComponentModel;
using Neuroflow.Networks.Neural.Learning;
using Neuroflow.Networks.Neural;
using Microsoft.VisualBasic.Activities;
using System.Activities.Expressions;

namespace Neuroflow.Activities.Networks.Neural.Learning
{
    [Designer(Designers.TrainingIterationDesigner)]
    public abstract class TrainingIteration : NativeActivity<BatchExecutionResult>, IActivityTemplateFactory
    {
        protected abstract NNAlgorithm Algorithm { get; }
        
        [RequiredArgument]
        [Category(PropertyCategories.Algorithm)]
        public InArgument<NeuralNetwork> Network { get; set; }

        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Get Next Batch")]
        public ActivityFunc<NeuralVectorFlowBatch> GetNextBatch { get; set; }

        internal ExecuteNeuralSupervisedBatch ExecuteTraining { get; private set; }

        Variable<NeuralVectorFlowBatch> batchResult;

        Variable<NeuralComputationContext> neuralCompContext;

        Variable<VectorBuffer<float>> vectorBuffer;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (GetNextBatch.IsNull()) metadata.AddValidationError("GetNextBatch function is required.");

            RuntimeArgument arg;

            metadata.AddArgument(arg = new RuntimeArgument("Network", typeof(NeuralNetwork), ArgumentDirection.In, true));
            metadata.Bind(Network, arg);

            metadata.AddDelegate(GetNextBatch);

            batchResult = new Variable<NeuralVectorFlowBatch>();
            metadata.AddImplementationVariable(batchResult);

            neuralCompContext = new Variable<NeuralComputationContext>();
            metadata.AddImplementationVariable(neuralCompContext);

            vectorBuffer = new Variable<VectorBuffer<float>>();
            metadata.AddImplementationVariable(vectorBuffer);

            ExecuteTraining =
                new ExecuteNeuralSupervisedBatch
                {
                    Algorithm = Algorithm,
                    Network = new ArgumentValue<NeuralNetwork>("Network"),
                    Batch = new InArgument<NeuralVectorFlowBatch>(batchResult),
                    NCContext = new InArgument<NeuralComputationContext>(neuralCompContext),
                    VectorBuffer = new InArgument<VectorBuffer<float>>(vectorBuffer),
                    Result = new ArgumentReference<BatchExecutionResult>("Result")
                };

            metadata.AddImplementationChild(ExecuteTraining);
        }

        protected override void Execute(NativeActivityContext context)
        {
            context.ScheduleFunc(GetNextBatch, OnGetNextBatchCompleted);
        }

        private void OnGetNextBatchCompleted(NativeActivityContext context, ActivityInstance instance, NeuralVectorFlowBatch batch)
        {
            if (batch != null)
            {
                var network = Network.Get(context);

                var vars = ComputationContext.GetVariables(context, network.UID.ToString());
                
                NeuralComputationContext ctx;
                string ctxName = "NeuralComputationContext";
                if (!vars.TryGet(ctxName, out ctx))
                {
                    ctx = network.CreateContext();
                    vars.Set(ctxName, ctx);
                }

                VectorBuffer<float> vbuff;
                string vbuffName = "VectorBuffer";
                if (!vars.TryGet(vbuffName, out vbuff))
                {
                    vbuff = network.CreateVectorBuffer();
                    vars.Set(vbuffName, vbuff);
                }

                neuralCompContext.Set(context, ctx);
                batchResult.Set(context, batch);
                vectorBuffer.Set(context, vbuff);

                context.ScheduleActivity(ExecuteTraining);
            }
            else
            {
                throw new InvalidOperationException("Training Iteration Get Next Batch Activity Method has returned a null value.");
            }
        }

        #region IActivityTemplateFactory Members

        Activity IActivityTemplateFactory.Create(System.Windows.DependencyObject target)
        {
            return CreateActivityTemplate();
        }

        protected abstract Activity CreateActivityTemplate();

        #endregion
    }
}
