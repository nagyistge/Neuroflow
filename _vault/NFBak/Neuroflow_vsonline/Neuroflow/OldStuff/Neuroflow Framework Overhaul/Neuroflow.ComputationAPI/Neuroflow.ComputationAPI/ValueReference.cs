using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.ComputationAPI
{
    public sealed class ValueReference<T> where T : struct
    {
        public ValueReference(ValueSpace<T> valueSpace)
        {
            Contract.Requires(valueSpace != null);

            this.valueSpace = valueSpace;
            ValueIndex = valueSpace.Declare();
        }

        public ValueReference(ValueSpace<T> valueSpace, int valueIndex)
        {
            Contract.Requires(valueSpace != null);
            Contract.Requires(valueIndex >= 0);

            this.valueSpace = valueSpace;
            ValueIndex = valueIndex;
        }
        
        ValueSpace<T> valueSpace;

        public int ValueIndex { get; private set; }

        public T Value
        {
            get { return valueSpace[ValueIndex]; }
            set { valueSpace[ValueIndex] = value; }
        }

        public string Ref
        {
            get { return valueSpace.Ref[ValueIndex]; }
        }
    }
}
