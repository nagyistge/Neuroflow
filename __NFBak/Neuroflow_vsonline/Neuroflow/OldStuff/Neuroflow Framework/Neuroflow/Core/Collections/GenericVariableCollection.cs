using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.Collections
{
    [Serializable]
    public sealed class GenericVariableCollection : IDisposable
    {
        Dictionary<string, object> variables = new Dictionary<string, object>();

        public void Set(string name, object value)
        {
            Contract.Requires(!String.IsNullOrEmpty(name));

            variables[name] = value;
        }

        public T Get<T>(string name)
        {
            Contract.Requires(!String.IsNullOrEmpty(name));

            object v;
            if (variables.TryGetValue(name, out v))
            {
                if (v is T) return (T)v;
                throw new InvalidOperationException("Variable '" + name + "' is not a(n) '" + typeof(T).FullName + "' object.");
            }
            throw new InvalidOperationException("Variable '" + name + "' does not exists.");
        }

        public bool TryGet<T>(string name, out T value)
        {
            Contract.Requires(!String.IsNullOrEmpty(name));

            object v;
            if (variables.TryGetValue(name, out v))
            {
                if (v is T)
                {
                    value = (T)v;
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        public bool Exists(string name)
        {
            Contract.Requires(!String.IsNullOrEmpty(name));

            return variables.ContainsKey(name);
        }

        public void Delete(string name)
        {
            Contract.Requires(!String.IsNullOrEmpty(name));

            object v;
            if (variables.TryGetValue(name, out v))
            {
                var d = v as IDisposable;
                if (d != null) d.Dispose();
                variables.Remove(name);
            }
        }

        public void Dispose()
        {
            if (variables != null)
            {
                foreach (var d in variables.Values.OfType<IDisposable>())
                {
                    d.Dispose();
                }

                variables = null;
                GC.SuppressFinalize(this);
            }
        }
    }
}
