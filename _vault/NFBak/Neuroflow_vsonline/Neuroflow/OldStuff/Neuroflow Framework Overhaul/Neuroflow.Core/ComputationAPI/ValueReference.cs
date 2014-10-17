using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.ComputationAPI
{
    public sealed class ValueReference<T> where T : struct
    {
        public ValueReference(ValueSpace<T> valueSpace)
        {
            Contract.Requires(valueSpace != null);

            this.valueSpace = valueSpace;
            ValueIndex = valueSpace.Declare();
            Ref = valueSpace.Ref[ValueIndex];
        }

        public ValueReference(ValueSpace<T> valueSpace, int valueIndex)
        {
            Contract.Requires(valueSpace != null);
            Contract.Requires(valueIndex >= 0);

            this.valueSpace = valueSpace;
            ValueIndex = valueIndex;
            Ref = valueSpace.Ref[ValueIndex];
        }
        
        ValueSpace<T> valueSpace;

        public int ValueIndex { get; private set; }

        public T Value
        {
            get { return valueSpace[ValueIndex]; }
            set { valueSpace[ValueIndex] = value; }
        }

        public string Ref { get; private set; }
    }
}
