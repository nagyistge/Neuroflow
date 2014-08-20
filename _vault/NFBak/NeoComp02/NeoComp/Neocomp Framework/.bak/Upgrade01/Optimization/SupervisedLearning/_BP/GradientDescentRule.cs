using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Adjustables;
using NeoComp.Networks2.Computational;
using NeoComp.Networks2.Computational.Neural;

namespace NeoComp.Optimization.SupervisedLearning
{
    public sealed class GradientDescentRule : BackPropagationRule
    {
        class DeltaContext
        {
            internal double? Delta { get; set; }
        }

        class ErrorContext : DeltaContext
        {
            internal double Error { get; set; }
        }
        
        public GradientDescentRule(IGDRuleParameters defaultParameters)
            : base(defaultParameters)
        {
            Contract.Requires(defaultParameters != null);
        }

        new public IGDRuleParameters DefaultParameters
        {
            get { return (IGDRuleParameters)base.DefaultParameters; }
        }

        protected internal override object CreateRuleContext(ComputationalNode<double> forNode)
        {
            if (forNode is ActivationNeuron) return new DeltaContext();
            return base.CreateRuleContext(forNode);
        }

        protected internal override object CreateRuleContext(ComputationalConnection<double> forConnection)
        {
            return new ErrorContext();
        }

        protected internal override void Initialize(ComputationalConnection<double> connection, object ruleContext)
        {
            base.Initialize(connection, ruleContext);
            Initialize(ruleContext);
        }

        protected internal override void Initialize(ComputationalNode<double> node, object ruleContext)
        {
            base.Initialize(node, ruleContext);
            Initialize(ruleContext);
        }

        private void Initialize(object ruleContext)
        {
            var ctx = ruleContext as DeltaContext;
            if (ctx != null) ctx.Delta = null;
        }

        protected internal override void InitializeOutputConnection(BackPropagationConnectionContext connCtx, double error)
        {
            ((ErrorContext)connCtx.RuleContext).Error = error;
        }

        protected internal override void BackPropagationStep(BackPropagationNetworkEntryContext context)
        {
            var nodeCtx = context.NodeContext;
            var lowerCtxs = context.LowerConnectionContexts;
            var upperCtxs = context.UpperConnectionContexts;

            double error = 0.0;
            foreach (var lCtx in lowerCtxs)
            {
                double currentError = ((ErrorContext)lCtx.RuleContext).Error;
                var syn = lCtx.Connection as Synapse;
                if (syn != null)
                {
                    error += syn.Weight * currentError;
                }
                else
                {
                    error += currentError;
                }
            }

            var neuron = nodeCtx.Node as ActivationNeuron;
            if (neuron != null)
            {
                error = neuron.ActivationFunction.Derivate(neuron.OutputValue) * error;
                Adjust((IAdjustableItem)neuron, error, 1.0, (DeltaContext)nodeCtx.RuleContext);
            }

            foreach (var uCtx in upperCtxs)
            {
                var currentContext = ((ErrorContext)uCtx.RuleContext);
                currentContext.Error = error;
                var synapse = uCtx.Connection as Synapse;
                if (synapse != null) Adjust((IAdjustableItem)synapse, error, synapse.InputValue, currentContext);
            }
        }

        private void Adjust(IAdjustableItem item, double error, double input, DeltaContext context)
        {
            var pars = GetCurrentParameters(item);
            double delta = pars.StepSize * error * input;
            if (pars.Momentum != 0.0)
            {
                if (context.Delta == null)
                {
                    context.Delta = delta;
                }
                else
                {
                    context.Delta = delta = (pars.Momentum * context.Delta.Value) + ((1.0 - pars.Momentum) * delta);
                }
            }
            item.Adjustment += delta;
        }

        private IGDRuleParameters GetCurrentParameters(IAdjustableItem item)
        {
            var pars = item as IGDRuleParameters;
            return pars != null ? pars : DefaultParameters;
        }
    }
}
