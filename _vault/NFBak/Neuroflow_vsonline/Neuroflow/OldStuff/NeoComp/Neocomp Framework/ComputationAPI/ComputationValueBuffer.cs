using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace ComputationAPI
{
    public sealed class ComputationValueBuffer<T>
        where T : struct
    {
        const int PageSize = 128;

        public ComputationValueBuffer()
        {
            Buffer = new T[PageSize];
        }

        internal T[] Buffer { get; private set; }

        int size = 0;

        public int Size
        {
            [Pure]
            get { return size; }
        }

        public ComputationValue<T> Declare()
        {
            Contract.Ensures(Contract.Result<ComputationValue<T>>() != null);
            
            lock (Buffer)
            {
                if (size == Buffer.Length)
                {
                    T[] newBuffer = new T[Buffer.Length + PageSize];
                    Array.Copy(Buffer, newBuffer, Buffer.Length);
                    Buffer = newBuffer;
                }

                return new ComputationValue<T>(this, size++);
            }
        }

        public ComputationValue<T>[] Declare(int count)
        {
            Contract.Requires(count > 0);
            Contract.Ensures(Contract.Result<ComputationValue<T>[]>() != null);
            Contract.Ensures(Contract.Result<ComputationValue<T>[]>().Length == count);

            lock (Buffer)
            {
                if (size + count >= Buffer.Length)
                {
                    T[] newBuffer = new T[size + count];
                    Array.Copy(Buffer, newBuffer, Buffer.Length);
                    Buffer = newBuffer;
                }

                var result = new ComputationValue<T>[count];
                for (int idx = 0; idx < count; idx++)
                {
                    result[idx] = new ComputationValue<T>(this, size++);
                }
                return result;
            }
        }

        public Action Compile(params Expression[] expressions)
        {
            Contract.Requires(expressions != null);
            Contract.Requires(expressions.Length > 0);

            if (expressions.Length == 1) return Expression.Lambda<Action>(expressions[0]).Compile();

            Action[] actions = new Action[expressions.Length];
            for (int idx = 0; idx < actions.Length; idx++)
            {
                actions[idx] = Expression.Lambda<Action>(expressions[idx]).Compile();
            }

            return () =>
            {
                for (int idx = 0; idx < actions.Length; idx++)
                {
                    actions[idx]();
                }
            };
        }
    }
}
