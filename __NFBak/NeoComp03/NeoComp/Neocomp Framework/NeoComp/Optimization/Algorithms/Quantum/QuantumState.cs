using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics;

namespace NeoComp.Optimization.Algorithms.Quantum
{
    public struct QuantumState : IEquatable<QuantumState>, IComparable<QuantumState>, IComparable
    {
        #region Constructors

        public QuantumState(QuantumState state)
        {
            this.value = state.value;
        }

        public QuantumState(double value)
        {
            if (value < 0.0) value = 0.0; else if (value > 1.0) value = 1.0;
            this.value = value;
        }

        public QuantumState(bool value)
        {
            this.value = value ? 1.0 : 0.0;
        } 

        #endregion

        #region Create

        public static QuantumState Random()
        {
            return RandomGenerator.Random.NextDouble();
        }

        public double ToRange(double min, double max)
        {
            double d = max - min;
            return (value * d) + min;
        }

        #endregion

        #region Fields

        double value; 

        #endregion

        #region Stuff

        public void Blend(QuantumState other)
        {
            double d = other.value - this.value;
            this.value += d / 2.0;
        }

        #endregion

        #region Casting Operators

        public static implicit operator double(QuantumState state)
        {
            return state.value;
        }

        public static implicit operator bool(QuantumState state)
        {
            return state.value < 0.5 ? false : true;
        }

        public static implicit operator QuantumState(double value)
        {
            return new QuantumState(value);
        }

        public static implicit operator QuantumState(bool value)
        {
            return new QuantumState(value);
        }

        #endregion

        #region Operators

        public static bool operator ==(QuantumState s1, QuantumState s2)
        {
            return s1.Equals(s2);
        }

        public static bool operator !=(QuantumState s1, QuantumState s2)
        {
            return !s1.Equals(s2);
        }

        public static bool operator <(QuantumState s1, QuantumState s2)
        {
            return s1.CompareTo(s2) < 0;
        }

        public static bool operator >(QuantumState s1, QuantumState s2)
        {
            return s1.CompareTo(s2) > 0;
        }

        public static bool operator <=(QuantumState s1, QuantumState s2)
        {
            return s1.CompareTo(s2) <= 0;
        }

        public static bool operator >=(QuantumState s1, QuantumState s2)
        {
            return s1.CompareTo(s2) >= 0;
        }

        #endregion

        #region IComparable<Impulse> Members

        public int CompareTo(QuantumState other)
        {
            return value.CompareTo(other.value);
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return CompareTo(Args.Cast<QuantumState>(obj, "obj"));
        }

        #endregion

        #region IEquatable<Impulse> Members

        public bool Equals(QuantumState other)
        {
            return value == other.value;
        }

        #endregion

        #region Object

        public override bool Equals(object obj)
        {
            return obj is QuantumState ? Equals((QuantumState)obj) : false;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        #endregion        
    }
}
