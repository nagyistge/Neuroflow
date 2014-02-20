using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Optimization.Learning
{
    public struct SolutionKey : IComparable<SolutionKey>, IEquatable<SolutionKey>
    {
        public SolutionKey(double mse)
        {
            this.mse = mse;
            this.uid = Guid.NewGuid();
        }

        private Guid uid;

        private double mse;

        public double MSE
        {
            get { return mse; }
        }

        #region Object

        public override bool Equals(object obj)
        {
            return obj is SolutionKey ? Equals((SolutionKey)obj) : false;
        }

        public override int GetHashCode()
        {
            return uid.GetHashCode();
        }

        public override string ToString()
        {
            return "MSE: " + mse;
        }

        #endregion

        #region IComparable<SolutionKey> Members

        public int CompareTo(SolutionKey other)
        {
            int c = mse.CompareTo(other.mse);
            if (c == 0) c = uid.CompareTo(other.uid);
            return c;
        }

        #endregion

        #region IEquatable<SolutionKey> Members

        public bool Equals(SolutionKey other)
        {
            return uid == other.uid;
        }

        #endregion
    } 
}
