using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    [ContractClass(typeof(IReinforcementLearningStrategyContract))]
    public interface IReinforcementLearningStrategy
    {
        void Reward(double reward);

        void Punishment(double punishment);
    }

    [ContractClassFor(typeof(IReinforcementLearningStrategy))]
    class IReinforcementLearningStrategyContract : IReinforcementLearningStrategy
    {
        void IReinforcementLearningStrategy.Reward(double reward)
        {
            Contract.Requires(reward >= 0.0 && reward <= 1.0);
        }

        void IReinforcementLearningStrategy.Punishment(double punishment)
        {
            Contract.Requires(punishment >= 0.0 && punishment <= 1.0);
        }
    }
}
