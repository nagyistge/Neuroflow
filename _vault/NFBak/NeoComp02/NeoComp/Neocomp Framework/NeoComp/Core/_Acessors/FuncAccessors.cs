using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Core
{
    public interface IFuncAccessor : IMethodAccessor
    {
        object Call(object instance, params object[] parameters);

        object FastCall(object instance, params object[] parameters);
    }

    public abstract class FuncAccessorBase<T, TResult> : MethodAccessorBase<T>, IFuncAccessor
    {
        #region IFuncAccessor Members

        object IFuncAccessor.Call(object instance, params object[] parameters)
        {
            CheckCallParameters(parameters);
            return ((IFuncAccessor)this).FastCall(instance, parameters);
        }

        object IFuncAccessor.FastCall(object instance, params object[] parameters)
        {
            T i = object.ReferenceEquals(instance, null) ? default(T) : (T)instance;
            if (parameters == null)
            {
                return DoCall(i);
            }
            switch (parameters.Length)
            {
                default:
                    return DoCall(i);
                case 1:
                    return DoCall(i, parameters[0]);
                case 2:
                    return DoCall(i, parameters[0], parameters[1]);
                case 3:
                    return DoCall(i, parameters[0], parameters[1], parameters[2]);
                case 4:
                    return DoCall(i, parameters[0], parameters[1], parameters[2], parameters[3]);
                case 5:
                    return DoCall(i, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4]);
                case 6:
                    return DoCall(i, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                case 7:
                    return DoCall(i, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6]);
                case 8:
                    return DoCall(i, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7]);
            }
        }

        #endregion

        public FuncAccessorBase(string name, params Type[] parTypes)
            : base(name, parTypes)
        {
        }

        protected virtual TResult DoCall(T instance)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual TResult DoCall(T instance, object p1)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual TResult DoCall(T instance, object p1, object p2)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual TResult DoCall(T instance, object p1, object p2, object p3)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual TResult DoCall(T instance, object p1, object p2, object p3, object p4)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual TResult DoCall(T instance, object p1, object p2, object p3, object p4, object p5)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual TResult DoCall(T instance, object p1, object p2, object p3, object p4, object p5, object p6)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual TResult DoCall(T instance, object p1, object p2, object p3, object p4, object p5, object p6, object p7)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual TResult DoCall(T instance, object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8)
        {
            throw new InvalidOperationException("Invlaid method call");
        }
    }

    public sealed class FuncAccessor<T, TResult> : FuncAccessorBase<T, TResult>
    {
        public FuncAccessor(string name)
            : base(name)
        {
        }

        public TResult Call(T instance)
        {
            Func<T, TResult> call;
            try
            {
                call = (Func<T, TResult>)Accessor;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(GetMethodNotFoundErrorMessage(null), ex);
            }
            return call(instance);
        }

        protected override TResult DoCall(T instance)
        {
            return Call(instance);
        }
    }

    public sealed class FuncAccessor<T, TP1, TResult> : FuncAccessorBase<T, TResult>
    {
        public FuncAccessor(string name)
            : base(name, typeof(TP1))
        {
        }

        public TResult Call(T instance, TP1 p1)
        {
            Func<T, TP1, TResult> call = (Func<T, TP1, TResult>)Accessor;
            return call(instance, p1);
        }

        protected override TResult DoCall(T instance, object p1)
        {
            return Call(instance, (TP1)p1);
        }
    }

    public sealed class FuncAccessor<T, TP1, TP2, TResult> : FuncAccessorBase<T, TResult>
    {
        public FuncAccessor(string name)
            : base(name, typeof(TP1), typeof(TP2))
        {
        }

        public TResult Call(T instance, TP1 p1, TP2 p2)
        {
            Func<T, TP1, TP2, TResult> call = (Func<T, TP1, TP2, TResult>)Accessor;
            return call(instance, p1, p2);
        }

        protected override TResult DoCall(T instance, object p1, object p2)
        {
            return Call(instance, (TP1)p1, (TP2)p2);
        }
    }

    public sealed class FuncAccessor<T, TP1, TP2, TP3, TResult> : FuncAccessorBase<T, TResult>
    {
        public FuncAccessor(string name)
            : base(name, typeof(TP1), typeof(TP2), typeof(TP3))
        {
        }

        public TResult Call(T instance, TP1 p1, TP2 p2, TP3 p3)
        {
            Func<T, TP1, TP2, TP3, TResult> call = (Func<T, TP1, TP2, TP3, TResult>)Accessor;
            return call(instance, p1, p2, p3);
        }

        protected override TResult DoCall(T instance, object p1, object p2, object p3)
        {
            return Call(instance, (TP1)p1, (TP2)p2, (TP3)p3);
        }
    }

    public sealed class FuncAccessor<T, TP1, TP2, TP3, TP4, TResult> : FuncAccessorBase<T, TResult>
    {
        public FuncAccessor(string name)
            : base(name, typeof(TP1), typeof(TP2), typeof(TP3), typeof(TP4))
        {
        }

        public TResult Call(T instance, TP1 p1, TP2 p2, TP3 p3, TP4 p4)
        {
            Func<T, TP1, TP2, TP3, TP4, TResult> call = (Func<T, TP1, TP2, TP3, TP4, TResult>)Accessor;
            return call(instance, p1, p2, p3, p4);
        }

        protected override TResult DoCall(T instance, object p1, object p2, object p3, object p4)
        {
            return Call(instance, (TP1)p1, (TP2)p2, (TP3)p3, (TP4)p4);
        }
    }

    public sealed class FuncAccessor<T, TP1, TP2, TP3, TP4, TP5, TResult> : FuncAccessorBase<T, TResult>
    {
        public FuncAccessor(string name)
            : base(name, typeof(TP1), typeof(TP2), typeof(TP3), typeof(TP4), typeof(TP5))
        {
        }

        public TResult Call(T instance, TP1 p1, TP2 p2, TP3 p3, TP4 p4, TP5 p5)
        {
            Func<T, TP1, TP2, TP3, TP4, TP5, TResult> call = (Func<T, TP1, TP2, TP3, TP4, TP5, TResult>)Accessor;
            return call(instance, p1, p2, p3, p4, p5);
        }

        protected override TResult DoCall(T instance, object p1, object p2, object p3, object p4, object p5)
        {
            return Call(instance, (TP1)p1, (TP2)p2, (TP3)p3, (TP4)p4, (TP5)p5);
        }
    }

    public sealed class FuncAccessor<T, TP1, TP2, TP3, TP4, TP5, TP6, TResult> : FuncAccessorBase<T, TResult>
    {
        public FuncAccessor(string name)
            : base(name, typeof(TP1), typeof(TP2), typeof(TP3), typeof(TP4), typeof(TP5), typeof(TP6))
        {
        }

        public TResult Call(T instance, TP1 p1, TP2 p2, TP3 p3, TP4 p4, TP5 p5, TP6 p6)
        {
            Func<T, TP1, TP2, TP3, TP4, TP5, TP6, TResult> call = (Func<T, TP1, TP2, TP3, TP4, TP5, TP6, TResult>)Accessor;
            return call(instance, p1, p2, p3, p4, p5, p6);
        }

        protected override TResult DoCall(T instance, object p1, object p2, object p3, object p4, object p5, object p6)
        {
            return Call(instance, (TP1)p1, (TP2)p2, (TP3)p3, (TP4)p4, (TP5)p5, (TP6)p6);
        }
    }

    public sealed class FuncAccessor<T, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TResult> : FuncAccessorBase<T, TResult>
    {
        public FuncAccessor(string name)
            : base(name, typeof(TP1), typeof(TP2), typeof(TP3), typeof(TP4), typeof(TP5), typeof(TP6), typeof(TP7))
        {
        }

        public TResult Call(T instance, TP1 p1, TP2 p2, TP3 p3, TP4 p4, TP5 p5, TP6 p6, TP7 p7)
        {
            Func<T, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TResult> call = (Func<T, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TResult>)Accessor;
            return call(instance, p1, p2, p3, p4, p5, p6, p7);
        }

        protected override TResult DoCall(T instance, object p1, object p2, object p3, object p4, object p5, object p6, object p7)
        {
            return Call(instance, (TP1)p1, (TP2)p2, (TP3)p3, (TP4)p4, (TP5)p5, (TP6)p6, (TP7)p7);
        }
    }

    public sealed class FuncAccessor<T, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TResult> : FuncAccessorBase<T, TResult>
    {
        public FuncAccessor(string name)
            : base(name, typeof(TP1), typeof(TP2), typeof(TP3), typeof(TP4), typeof(TP5), typeof(TP6), typeof(TP7), typeof(TP8))
        {
        }

        public TResult Call(T instance, TP1 p1, TP2 p2, TP3 p3, TP4 p4, TP5 p5, TP6 p6, TP7 p7, TP8 p8)
        {
            Func<T, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TResult> call = (Func<T, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8, TResult>)Accessor;
            return call(instance, p1, p2, p3, p4, p5, p6, p7, p8);
        }

        protected override TResult DoCall(T instance, object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8)
        {
            return Call(instance, (TP1)p1, (TP2)p2, (TP3)p3, (TP4)p4, (TP5)p5, (TP6)p6, (TP7)p7, (TP8)p8);
        }
    }
}
