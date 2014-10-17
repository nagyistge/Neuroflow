using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.Statistical
{
    public class ContinousOptUnit : OptUnit
    {
        #region Construct

        public ContinousOptUnit(string id, DoubleRange valueRange)
            : base(id)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));

            ValueRange = valueRange;
        }

        #endregion

        #region Properties

        public DoubleRange ValueRange { get; private set; }

        #endregion

        #region Impl

        protected internal override object CreateContext()
        {
            return new Entropy(0.0, Consts.MaxStdDev);
        }

        protected internal override EntityDataUnit CreateEntityDataUnit(object context)
        {
            var entropy = (Entropy)context;
            double entropyValue = Statistics.GenerateGauss(entropy.Mean, entropy.StdDev);
            return new EntityDataUnit(ID, Consts.MeanRange.Normalize(entropyValue, ValueRange));
        }

        protected internal override object ComputeNewContext(IEnumerable<EntityDataUnit> eliteEntityDataUnits)
        {
            double mean, stdDev;
            Statistics.CalculateMeanAndStdDev(
                eliteEntityDataUnits.Select(du => du.EntityData)
                    .Cast<double>()
                    .Select(v => Consts.MeanRange.Denormalize(v, ValueRange)), 
                out mean, out stdDev);

            mean = Consts.MeanRange.Cut(mean);
            if (double.IsNaN(stdDev) || stdDev < Consts.Epsilon) stdDev = Consts.Epsilon; else if (stdDev > Consts.MaxStdDev) stdDev = Consts.MaxStdDev;

            return new Entropy(mean, stdDev);
        }

        protected internal override EntityDataUnit CreateRandomEntityDataUnit()
        {
            return new EntityDataUnit(ID, ValueRange.PickRandomValue());
        }

        #endregion
    }
}
