using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Diagnostics.Contracts;

namespace ComputationAPI
{
    public sealed class ComputationValue<T>
        where T : struct
    {
        internal ComputationValue(ComputationValueBuffer<T> buffer, int index)
        {
            this.buffer = buffer;
            this.Index = index;
        }

        ComputationValueBuffer<T> buffer;

        public int Index { get; private set; }

        public T Value { get; set; }

        public T Value2
        {
            get { return buffer.Buffer[Index]; }
            set { buffer.Buffer[Index] = value; }
        }

        public Expression ValueExpression
        {
            get
            {
                var idxEx = Expression.Constant(Index);
                var arrEx = Expression.Constant(buffer.Buffer);
                return Expression.ArrayAccess(arrEx, idxEx);
            }
        }

        public Expression Assign(Expression value)
        {
            Contract.Requires(value != null);

            return Expression.Assign(ValueExpression, value);
        }
    }
}
