using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics.Contracts;
using Neuroflow.Core.Computations;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Neuroflow.Core.ComputationalNetworks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow)]
    public abstract class ComputationConnection<T> : IReset
        where T : struct
    {
        [DataMember(Name = "value", EmitDefaultValue = false)]
        internal ComputationValue<T> ConnectionValue { get; private set; }

        public T InputValue
        {
            get { return ConnectionValue != null ? ConnectionValue.Value : default(T); }
        }

        public virtual T OutputValue
        {
            get { return InputValue; }
        }

        internal void AdaptValue(ComputationValue<T> value)
        {
            Contract.Requires(value != null);

            ConnectionValue = value;
        }

        protected virtual void Reset()
        {
            if (ConnectionValue != null)
            {
                ConnectionValue.Value = default(T);
            }
        }

        #region IReset Members

        void IReset.Reset()
        {
            Reset();
        }

        #endregion
    }
}
