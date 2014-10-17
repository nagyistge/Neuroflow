using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NeoComp.Core;
using NeoComp.Computations;

namespace NeoComp.Networks
{
    public class ComputationalConnection<T> : Connection
    {
        #region Properties

        public ComputationalValue<T> ConnectionValue { get; private set; }

        #endregion

        #region Initialize

        internal void Initialize(ComputationalValue<T> value)
        {
            Debug.Assert(value != null);
            this.ConnectionValue = value;
        }

        #endregion

        #region Behavior

        protected internal virtual void Setup(T value)
        {
            Debug.Assert(ConnectionValue != null);
            ConnectionValue.Value = value;
        }

        #endregion
    }
}
