using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;

namespace NeoComp.Optimization.GeneticNetworks
{
    public struct ConfiguredNetworkGene<TData>
        where TData : struct
    {
        #region Constructor

        public ConfiguredNetworkGene(ConnectionIndex index, TData data)
        {
            this.index = index;
            this.data = data;
        }

        #endregion

        #region Properties

        ConnectionIndex index;

        public ConnectionIndex Index
        {
            get { return index; }
            set { index = value; }
        }

        public int NodeIndex
        {
            get { return Index.LowerNodeIndex; }
        }

        TData data;

        public TData Data
        {
            get { return data; }
        }

        #endregion
    }
}
