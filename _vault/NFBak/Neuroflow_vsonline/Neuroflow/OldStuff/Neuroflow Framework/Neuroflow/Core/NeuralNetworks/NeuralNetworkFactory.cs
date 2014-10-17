using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using Neuroflow.Core.ComputationalNetworks;
using Neuroflow.Core.Networks;

namespace Neuroflow.Core.NeuralNetworks
{
    public class NeuralNetworkFactory : ComputationalNetworkFactory<double>
    {
        #region Constructors

        public NeuralNetworkFactory(int inputInterfaceLength, int outputInterfaceLength)
            : base(inputInterfaceLength, outputInterfaceLength)
        {
            Contract.Requires(inputInterfaceLength > 0);
            Contract.Requires(outputInterfaceLength > 0);
        }

        public NeuralNetworkFactory(NeuralNetwork network)
            : base(network)
        {
            Contract.Requires(network != null);
        } 

        #endregion

        #region Override

        protected internal override bool OverrideNetworkEntry(ModifyableNetworkEntry<ComputationNode<double>, ComputationConnection<double>> networkEntry, HashSet<int> occupiedNodeIndexes)
        {
            if (base.OverrideNetworkEntry(networkEntry, occupiedNodeIndexes))
            {
                CheckConns(networkEntry.LowerConnectionEntries);
                CheckConns(networkEntry.UpperConnectionEntries);
                return true;
            }
            return false;
        }

        private void CheckConns(IList<ConnectionEntry<ComputationConnection<double>>> collection)
        {
            foreach (var entry in collection)
            {
                if (!(entry.Connection is INeuralConnection))
                {
                    throw new InvalidOperationException(
                        "Connection entry on " + entry.Index + " doesn't implement INeuralConnection interface.");
                }
            }
        }

        #endregion
    }
}
