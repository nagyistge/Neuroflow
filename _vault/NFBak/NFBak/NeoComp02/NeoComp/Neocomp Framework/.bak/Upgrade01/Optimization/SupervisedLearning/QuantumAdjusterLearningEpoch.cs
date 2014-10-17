using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks2.Computational.Neural;
using System.Diagnostics.Contracts;
using NeoComp.Optimization.Quantum;
using NeoComp.Adjustables;
using System.Diagnostics;

namespace NeoComp.Optimization.SupervisedLearning
{
    public abstract class QuantumAdjusterLearningEpoch : StochasticAdjusterLearningEpoch
    {
        #region Item Class

        class QSAI : IQuantumStatedItem
        {
            internal QSAI(IAdjustableItem item)
            {
                Contract.Requires(item != null);

                this.item = item;
                var limited = item as ILimitedAdjustableItem;
                double max;
                if (limited != null)
                {
                    min = limited.Min;
                    max = limited.Max;
                }
                else
                {
                    min = -1.0;
                    max = 1.0;
                }
                size = max - min;
            }

            double min, size;

            IAdjustableItem item;

            QuantumState IQuantumStatedItem.State
            {
                get { return (item.Adjustment - min) / size; }
                set { item.Adjustment = value * size + min; }
            }
        } 

        #endregion

        #region Constructor

        protected QuantumAdjusterLearningEpoch(NeuralNetwork network, NeuralNetworkTest test = null)
            : base(network, test)
        {
            Contract.Requires(network != null);
        } 

        #endregion

        #region Initialize

        protected sealed override void Initialize(IEnumerable<IAdjustableItem> items)
        {
            Initialize(items.Select(i => new QSAI(i)));
        }

        protected abstract void Initialize(IEnumerable<IQuantumStatedItem> items); 

        #endregion
    }
}
