using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    public struct BodyCount
    {
        #region Constructors

        public BodyCount(double percent)
        {
            Contract.Requires(percent >= 0.0 && percent <= 100);

            this.percent = percent;
            this.count = null;
        }

        public BodyCount(int count)
        {
            Contract.Requires(count >= 0);

            this.percent = null;
            this.count = count;
        } 

        #endregion

        #region Properties

        double? percent;

        public double? Percent
        {
            get { return percent; }
        }

        int? count;

        public int? Count
        {
            get { return count; }
        } 

        #endregion

        #region Operators

        public static implicit operator BodyCount(int count)
        {
            return new BodyCount(count);
        }

        public static implicit operator BodyCount(double percent)
        {
            return new BodyCount(percent);
        }

        #endregion

        #region GetCount

        internal int GetCount(int populationSize)
        {
            int result = 0;
            if (count.HasValue) result = count.Value <= populationSize ? count.Value : populationSize;
            if (percent.HasValue) result = (int)Math.Abs(((double)populationSize * percent.Value) / 100.0);
            return result != 0 ? result : 1;
        }

        #endregion
    }
}
