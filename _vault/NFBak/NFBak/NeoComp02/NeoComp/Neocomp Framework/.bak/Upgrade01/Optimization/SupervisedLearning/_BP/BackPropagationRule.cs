using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Adjustables;
using NeoComp.Networks2.Computational.Neural;
using NeoComp.Core;
using System.Collections;
using NeoComp.Networks2.Computational;

namespace NeoComp.Optimization.SupervisedLearning
{
    public abstract class BackPropagationRule
    {
        public BackPropagationRule(IRuleParameters defaultParameters)
        {
            Contract.Requires(defaultParameters != null);

            DefaultParameters = defaultParameters;
        }

        public IRuleParameters DefaultParameters { get; private set; }

        protected internal virtual void Initialize() { }

        protected internal virtual object CreateRuleContext(ComputationalNode<double> forNode)
        {
            return null;
        }

        protected internal virtual object CreateRuleContext(ComputationalConnection<double> forConnection)
        {
            return null;
        }

        protected internal virtual void Initialize(ComputationalNode<double> node, object ruleContext)
        {
            var ai = node as IAdjustableItem;
            if (ai != null) Initialize(ai);
        }

        protected internal virtual void Initialize(ComputationalConnection<double> connection, object ruleContext)
        {
            var ai = connection as IAdjustableItem;
            if (ai != null) Initialize(ai);
        }

        private void Initialize(IAdjustableItem item)
        {
            var pars = GetCurrentParameters(item);
            item.Adjustment = (pars.WeightInititlizationNoise * RandomGenerator.Random.NextDouble()) * 2.0 - pars.WeightInititlizationNoise;
        }

        protected internal abstract void InitializeOutputConnection(BackPropagationConnectionContext connCtx, double error);

        protected internal virtual void BeginBackPropagation(BackPropagationNetworkEntryContext[] contexts)
        {
        }

        protected internal virtual void EndBackPropagation(BackPropagationNetworkEntryContext[] contexts)
        {
        }

        protected internal abstract void BackPropagationStep(BackPropagationNetworkEntryContext context);

        private IRuleParameters GetCurrentParameters(IAdjustableItem item)
        {
            var pars = item as IRuleParameters;
            return pars != null ? pars : DefaultParameters;
        }
    }
}
