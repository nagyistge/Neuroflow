using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Collections.ObjectModel;

namespace NeoComp.Networks
{
    public abstract class ComputationalNode<TConnection, T> : Node
        where TConnection : ComputationalConnection<T>
    {
        #region Compute Output

        internal void ComputeOutputValue(TConnection[] inputConnections, TConnection[] outputConnections)
        {
            if (outputConnections.Length > 0)
            {
                T output;
                if (GenerateOutput(inputConnections, outputConnections, out output))
                {
                    SetupOutput(output, outputConnections);
                }
            }
        }

        protected abstract bool GenerateOutput(TConnection[] inputConnections, TConnection[] outputConnections, out T output);

        protected void SetupOutput(T value, TConnection[] outputConnections)
        {
            foreach (var conn in outputConnections)
            {
                conn.Setup(value);
            }
        } 

        #endregion
    }
}
