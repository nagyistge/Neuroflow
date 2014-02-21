using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class BackwardConnectionEntry
    {
        internal BackwardConnectionEntry(ConnectionIndex index, IBackwardConnection connection)
        {
            Contract.Requires(connection != null);

            Index = index;
            Connection = connection;
            bwValues = connection.BackwardValues;
            lastBwValue = bwValues.Last;
        }

        BackwardValues bwValues;

        BackwardValue lastBwValue;
        
        public ConnectionIndex Index { get; private set; }

        public IBackwardConnection Connection { get; private set; }

        public double LastWeightedError
        {
            get { return lastBwValue.Error * Connection.Weight; }
        }

        public void AddNextError(double error, bool isNewBatch)
        {
            bwValues.AddNext(error, Connection.InputValue, isNewBatch);
        }
    }
}
