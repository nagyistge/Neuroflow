using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Neuroflow.Core.Optimizations;
using Neuroflow.Core.NeuralNetworks.Learning;
using Neuroflow.Core.Optimizations.NeuralNetworks;

namespace Neuroflow.Activities.NeuralNetworks
{
    internal sealed class ExecuteTraining : AsyncCodeActivity<BatchExecutionResult>
    {
        internal bool DoValidationOnly { get; set; }
        
        [RequiredArgument]
        public InArgument<Training> Training { get; set; }

        [RequiredArgument]
        public InArgument<NeuralBatch> Batch { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            var training = Training.Get(context);
            var batch = Batch.Get(context);

            if (DoValidationOnly)
            {
                Func<Validation, NeuralBatch, BatchExecutionResult> work = (v, b) =>
                {
                    var result = v.Execute(b);//, (r) => Console.WriteLine(r.AverageError));
                    return result;
                };

                context.UserState = work;

                return work.BeginInvoke(training.CreateValidation(), batch, callback, state);
            }
            else
            {
                Func<Training, NeuralBatch, BatchExecutionResult> work = (t, b) =>
                {
                    var result = t.Execute(b);//, (r) => Console.WriteLine(r.AverageError));
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
                return ((Func<Validation, NeuralBatch, BatchExecutionResult>)context.UserState).EndInvoke(result);
            }
            else
            {
                return ((Func<Training, NeuralBatch, BatchExecutionResult>)context.UserState).EndInvoke(result);
            }
        }
    }
}
