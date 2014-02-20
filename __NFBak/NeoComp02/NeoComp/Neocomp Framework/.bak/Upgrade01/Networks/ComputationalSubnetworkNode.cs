using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NeoComp.Core;
using System.Collections.ObjectModel;

namespace NeoComp.Networks
{
    internal sealed class ComputationalSubnetworkNode<TConnection, T> : ComputationalNode<TConnection, T>
        where TConnection : ComputationalConnection<T>
    {
        #region Constructor

        internal ComputationalSubnetworkNode(ComputationalNetwork<TConnection, T> innerNetwork)
        {
            Debug.Assert(innerNetwork != null);
            InnerNetwork = innerNetwork;
        } 

        #endregion

        #region Properties

        internal ComputationalNetwork<TConnection, T> InnerNetwork { get; private set; } 

        #endregion

        #region Compute

        protected override bool GenerateOutput(TConnection[] inputConnections, TConnection[] outputConnections, out T output)
        {
            SetupInput(inputConnections);

            InnerNetwork.GenerateOutput();

            SetupOutput(outputConnections);

            output = default(T);
            return false;
        }

        private void SetupInput(TConnection[] inputConnections)
        {
            int count = Math.Min(InnerNetwork.InputInterface.Length, inputConnections.Length);
            for (int idx = 0; idx < count; idx++)
            {
                InnerNetwork.InputInterface[idx] = inputConnections[idx].ConnectionValue.Value;
            }
        }

        private void SetupOutput(TConnection[] outputConnections)
        {
            int count = Math.Min(InnerNetwork.OutputInterface.Length, outputConnections.Length);
            for (int idx = 0; idx < count; idx++)
            {
                outputConnections[idx].Setup(InnerNetwork.OutputInterface[idx]);
            }
        }

        #endregion
    }
}
