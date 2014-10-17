using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.Diagnostics;

namespace Neuroflow.Core.Internal
{
    public interface IAccessor
    {
        string Name { get; }

        Type DeclaringType { get; }

        MemberInfo MemberInfo { get; }
    }

    public class Accessor<T> : IAccessor, IEquatable<Accessor<T>>
    {
        public Accessor(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("", "name");
            this.Name = name;
        }

        public string Name { get; private set; }

        public Type DeclaringType
        {
            get { return typeof(T); }
        }

        public MemberInfo MemberInfo
        {
            get;
            protected set;
        }

        protected ParameterExpression GetInstanceParameter()
        {
            return Expression.Parameter(typeof(T), "instance");
        }

        protected ParameterExpression GetParameter<TParameter>(string name)
        {
            return Expression.Parameter(typeof(TParameter), name);
        }

        protected static string GetInstanceTypeErrorMessage()
        {
            return string.Format("Instance argument type is not '{0}'.", typeof(T).FullName);
        }

        protected bool IsObjectOkForType(object obj, Type type)
        {
            Debug.Assert(type != null);
            if (object.ReferenceEquals(obj, null))
            {
                return !type.IsValueType;
            }
            else
            {
                return obj.GetType().IsA(type);
            }
        }

        public override int GetHashCode()
        {
            return MemberInfo.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return MemberInfo.Equals(obj);
        }

        #region IEquatable<Acessor<T>> Members

        public bool Equals(Accessor<T> other)
        {
            if (other == null) return false;
            if (GetType() != other.GetType()) return false;
            return MemberInfo.Equals(other.MemberInfo);
        }

        #endregion
    }
}
