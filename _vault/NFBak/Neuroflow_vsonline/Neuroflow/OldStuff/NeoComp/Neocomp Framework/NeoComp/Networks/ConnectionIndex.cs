﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Networks
{
    [Serializable]
    [DataContract(Namespace = NeoComp.xmlns, Name = "connIdx")]
    public struct ConnectionIndex : IEquatable<ConnectionIndex>, IComparable<ConnectionIndex>, IComparable
    {
        public ConnectionIndex(int upperNodeIndex, int lowerNodeIndex)
        {
            Contract.Requires(upperNodeIndex >= 0);
            Contract.Requires(lowerNodeIndex >= 0);
            this.upperNodeIndex = upperNodeIndex;
            this.lowerNodeIndex = lowerNodeIndex;
        }

        [DataMember(Name = "uidx")]
        int upperNodeIndex;

        public int UpperNodeIndex
        {
            get { return upperNodeIndex; }
        }

        [DataMember(Name = "lidx")]
        int lowerNodeIndex;

        public int LowerNodeIndex
        {
            get { return lowerNodeIndex; }
        }

        #region IEquatable<ConnectionIndex> Members

        public bool Equals(ConnectionIndex other)
        {
            return upperNodeIndex == other.upperNodeIndex && lowerNodeIndex == other.lowerNodeIndex;
        }

        #endregion

        #region Object

        public override bool Equals(object obj)
        {
            return obj is ConnectionIndex ? Equals((ConnectionIndex)obj) : false;
        }

        public override int GetHashCode()
        {
            return upperNodeIndex ^ lowerNodeIndex;
        }

        public override string ToString()
        {
            return upperNodeIndex.ToString() + "," + lowerNodeIndex.ToString();
        }

        #endregion

        #region IComparable<ConnectionIndex> Members

        public int CompareTo(ConnectionIndex other)
        {
            int c = upperNodeIndex.CompareTo(other.upperNodeIndex);
            if (c == 0) c = lowerNodeIndex.CompareTo(other.lowerNodeIndex);
            return c;
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return CompareTo(Args.Cast<ConnectionIndex>(obj, "obj"));
        }

        #endregion

        #region Operators

        public static bool operator ==(ConnectionIndex index1, ConnectionIndex index2)
        {
            return index1.Equals(index2);
        }

        public static bool operator !=(ConnectionIndex index1, ConnectionIndex index2)
        {
            return !index1.Equals(index2);
        }

        public static bool operator <(ConnectionIndex index1, ConnectionIndex index2)
        {
            return index1.CompareTo(index2) < 0;
        }

        public static bool operator >(ConnectionIndex index1, ConnectionIndex index2)
        {
            return index1.CompareTo(index2) > 0;
        }

        public static bool operator <=(ConnectionIndex index1, ConnectionIndex index2)
        {
            return index1.CompareTo(index2) <= 0;
        }

        public static bool operator >=(ConnectionIndex index1, ConnectionIndex index2)
        {
            return index1.CompareTo(index2) >= 0;
        }

        #endregion
    }
}
