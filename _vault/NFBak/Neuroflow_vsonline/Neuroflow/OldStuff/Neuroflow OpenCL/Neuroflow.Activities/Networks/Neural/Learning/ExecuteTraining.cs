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
    internal sealed class ExecuteTraining : AsyncCodeActivity<BatchExecutionResult>
    {
        internal bool DoValidationOnly { get; set; }

        [RequiredArgument]
        public InArgument<Training> Training { get; set; }

        [RequiredArgument]
        public InArgument<NeuralVectorFlowBatch> Batch { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            var training = Training.Get(context);
            var batch = Batch.Get(context);

            if (DoValidationOnly)
            {
                Func<Validation, NeuralVectorFlowBatch, BatchExecutionResult> work = (v, b) =>
                {
                    var result = v.DoBatchExcuteIterations(b);//, (r) => Console.WriteLine(r.AverageError));
                    return result;
                };

                context.UserState = work;

                return work.BeginInvoke(training.CreateValidation(), batch, callback, state);
            }
            else
            {
                Func<Training, NeuralVectorFlowBatch, BatchExecutionResult> work = (t, b) =>
                {
                    var result = t.DoBatchExcuteIterations(b);//, (r) => Console.WriteLine(r.AverageError));
                    return result;
                };

                context.UserState = work;

                return work.BeginInvoke(training, batch, callback, state);
            }
        }

        protected override BatchExecutionResult EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            if (DoValidationOnly)
            {
                return ((Func<Validation, NeuralVectorFlowBatch, BatchExecutionResult>)context.UserState).EndInvoke(result);
            }
            else
            {
                return ((Func<Training, NeuralVectorFlowBatch, BatchExecutionResult>)context.UserState).EndInvoke(result);
            }
        }
    }
}
