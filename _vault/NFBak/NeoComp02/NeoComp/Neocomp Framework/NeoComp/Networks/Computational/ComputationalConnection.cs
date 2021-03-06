﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using NeoComp.Computations;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace NeoComp.Networks.Computational
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns)]
    public abstract class ComputationalConnection<T> : IReset
        where T : struct
    {
        //[DataMember(Name = "value", EmitDefaultValue = false)]
        //ComputationValue<T> connectionValue;

        //internal ComputationValue<T> ConnectionValue
        //{
        //    get { return connectionValue ?? (connectionValue = new ComputationValue<T>()); }
        //}

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
