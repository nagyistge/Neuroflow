using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using NeoComp.Optimization.Algorithms.Quantum;

namespace NeoComp.Optimization.Learning
{
    [ContractClass(typeof(MSEAdaptiveQuantumAnnealingContract))]
    public abstract class MSEAdaptiveQuantumAnnealing : AdaptiveQuantumAnnealing
    {
        protected MSEAdaptiveQuantumAnnealing(IEnumerable<IQuantumStatedItem> items)
            : base(items)
        {
            Contract.Requires(items != null);

            InitConstants();
        }

        double acceptTresholdInc, mseSplitPoint, acceptTreshold;

        private void InitConstants()
        {
            double d = SolutionVectorDimension;
            acceptTresholdInc = GetAcceptTresholdIncConstant();
            mseSplitPoint = GetMSESplitPointConstant();
        }

        protected virtual double GetAcceptTresholdIncConstant()
        {
            Contract.Ensures(Contract.Result<double>() >= 0.0 && Contract.Result<double>() <= 1.0);

            return Math.Min(Math.Pow(10.0, -Math.Log(SolutionVectorDimension)) * 1.5, 0.00011);
        }

        protected virtual double GetMSESplitPointConstant()
        {
            Contract.Ensures(Contract.Result<double>() >= 0.0 && Contract.Result<double>() <= 1.0);

            return Math.Min(1.0, (1.0 / SolutionVectorDimension) * 1000.0);
        }

        protected abstract double ComputeMSE();

        public override void Reset()
        {
            base.Reset();
            acceptTreshold = 0.0;
        }

        protected sealed override double ComputeEnergy()
        {
            return ComputeMSE();
        }

        protected sealed override double GetTunnelingFieldStrength(double currentEnergy)
        {
            return (currentEnergy * (1.0 - mseSplitPoint) + Math.Sqrt(currentEnergy) * mseSplitPoint);
        }

        protected sealed override double GetWrongSolutionAcceptTreshold(double currentEnergy, double newEnergy)
        {
            double t = acceptTreshold * (1.0 - newEnergy);
            return t;
        }

        protected override void Improved()
        {
            acceptTreshold = 0.0;
        }

        protected override void Rejected()
        {
            acceptTreshold += acceptTresholdInc;
        }

        protected override QuantumState GetItemInitState(int itemIndex)
        {
            return RandomGenerator.Random.NextDouble();
            //return 0.5;
        }
    }

    [ContractClassFor(typeof(MSEAdaptiveQuantumAnnealing))]
    abstract class MSEAdaptiveQuantumAnnealingContract : MSEAdaptiveQuantumAnnealing
    {
        protected MSEAdaptiveQuantumAnnealingContract()
            : base(null)
        {
        }
        
        protected override double ComputeMSE()
        {
            Contract.Ensures(Contract.Result<double>() >= 0.0 && Contract.Result<double>() <= 1.0);
            return 0;
        }
    }
}
