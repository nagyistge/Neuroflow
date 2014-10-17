using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Linq.Expressions;

namespace NeoComp.Core
{
    public interface IMethodAccessor : IAccessor
    {
        Type[] ParameterTypes { get; }

        MethodInfo MethodInfo { get; }
    }
    
    public abstract class MethodAccessorBase<T> : Accessor<T>, IMethodAccessor
    {
        #region Delegates

        protected delegate void Action<T1, T2, T3, T4, T5>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5);

        protected delegate void Action<T1, T2, T3, T4, T5, T6>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 p6);

        protected delegate void Action<T1, T2, T3, T4, T5, T6, T7>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 p6, T7 p7);

        protected delegate void Action<T1, T2, T3, T4, T5, T6, T7, T8>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 p6, T7 p7, T8 p8);

        protected delegate void Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 p6, T7 p7, T8 p8, T9 p9);

        protected delegate TR Func<T1, T2, T3, T4, T5, TR>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5);

        protected delegate TR Func<T1, T2, T3, T4, T5, T6, TR>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 p6);

        protected delegate TR Func<T1, T2, T3, T4, T5, T6, T7, TR>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 p6, T7 p7);

        protected delegate TR Func<T1, T2, T3, T4, T5, T6, T7, T8, TR>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 p6, T7 p7, T8 p8);

        protected delegate TR Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TR>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 p6, T7 p7, T8 p8, T9 p9); 

        #endregion

        #region IMethodAccessor Members

        public MethodInfo MethodInfo
        {
            get { return (MethodInfo)this.MemberInfo; }
        }

        #endregion

        public MethodAccessorBase(string name, params Type[] parTypes)
            : base(name)
        {
            if (parTypes == null) parTypes = Type.EmptyTypes;
            try
            {
                Accessor = CompileAccessor(parTypes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(GetMethodNotFoundErrorMessage(parTypes), ex);
            }
        }

        public Type[] ParameterTypes
        {
            get;
            private set;
        }

        protected Delegate Accessor { get; private set; }

        protected MethodInfo GetCallMethodInfo(params Type[] types)
        {
            var mi = GetMethod(Name, types);
            if (mi == null) throw new InvalidOperationException(GetMethodNotFoundErrorMessage(types));
            this.MemberInfo = mi;
            return mi;
        }

        protected MethodInfo GetMethod(string name, params Type[] types)
        {
            MethodInfo mi = typeof(T).GetMethod(name, types);
            if (mi == null)
            {
                try { mi = typeof(T).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance, null, types, null); }
                catch { };
            }
            if (mi == null)
            {
                try { mi = typeof(T).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static, null, types, null); }
                catch { };
            }

            return mi;
        }

        protected string GetMethodNotFoundErrorMessage(params Type[] parTypes)
        {
            if (parTypes == null) parTypes = ParameterTypes;
            StringBuilder sb = new StringBuilder();
            if (parTypes != null)
            {
                int idx = 0;
                foreach (var pt in parTypes)
                {
                    if (sb.Length != 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(pt.FullName);
                    sb.Append(' ');
                    sb.Append("par");
                    sb.Append(idx);
                    idx++;
                }
            }
            return string.Format("Method '{0}({1})' not found in '{2}'.", Name, sb.ToString(), typeof(T).FullName);
        }

        private Delegate CompileAccessor(Type[] parTypes)
        {
            ParameterTypes = parTypes;
            var pInstance = GetInstanceParameter();
            var pars = GetCallPars(parTypes);
            var eCall = MethodCallExpression.Call(pInstance, GetCallMethodInfo(parTypes), pars);
            var parList = pars.ToList();
            parList.Insert(0, pInstance);
            pars = parList.ToArray();
            return Expression.Lambda(eCall, pars).Compile();
        }

        private ParameterExpression[] GetCallPars(Type[] parTypes)
        {
            var result = new ParameterExpression[parTypes.Length];
            for (int idx = 0; idx < parTypes.Length; idx++)
            {
                Type type = parTypes[idx];
                Debug.Assert(type != null);
                var parExpr = Expression.Parameter(type, string.Format("p{0}", idx));
                result[idx] = parExpr;
            }
            return result;
        }

        protected void CheckCallParameters(object[] parameters)
        {
            if (parameters != null)
            {
                if (parameters.Length > 8) throw new ArgumentException("Too many parameters.", "parameters");
                if (parameters.Length != ParameterTypes.Length) throw new ArgumentException("Parameter count mismatch.", "parameters");
                for (int i = 0; i < parameters.Length; i++)
                {
                    object par = parameters[i];
                    Type methodParType = ParameterTypes[i];
                    if (!IsObjectOkForType(par, methodParType)) 
                        throw new ArgumentException(string.Format("Invalid parameter type. Index '{0}'.", i), "parameters");
                }
            }
        }
    }
}
