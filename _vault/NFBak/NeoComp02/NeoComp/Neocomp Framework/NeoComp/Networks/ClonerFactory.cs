using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks
{
    internal sealed class ClonerFactory<T> : IFactory<T>
    {
        internal ClonerFactory(T obj)
        {
            Contract.Requires(!object.ReferenceEquals(obj, null));

            this.obj = obj;
        }

        T obj;

        public T Create()
        {
            if (typeof(T).Implements<ICloneable>())
            {
                return MakeClone();
            }
            return obj;
        }

        private T MakeClone()
        {
            var c = (ICloneable)obj;
            try
            {
                return (T)c.Clone();
            }
            catch (Exception ex)
            {
                var newEx = new InvalidOperationException("Cannot make clone. See provided data 'type' and inner exception for details.", ex);
                newEx.Data["type"] = typeof(T);
                throw ex;
            }
        }

        object IFactory.Create()
        {
            return Create();
        }

        Type IFactory.ObjectType
        {
            get { return typeof(T); }
        }
    }
}
