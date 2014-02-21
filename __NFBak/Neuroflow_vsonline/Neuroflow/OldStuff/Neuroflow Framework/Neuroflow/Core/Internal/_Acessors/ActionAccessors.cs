using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Core.Internal
{
    public interface IActionAccessor : IMethodAccessor
    {
        void Call(object instance, params object[] parameters);

        void FastCall(object instance, params object[] parameters);
    }

    public abstract class ActionAccessorBase<T> : MethodAccessorBase<T>, IActionAccessor
    {
        #region IActionAccessor Members

        void IActionAccessor.Call(object instance, params object[] parameters)
        {
            CheckCallParameters(parameters);
            ((IActionAccessor)this).FastCall(instance, parameters);
        }

        void IActionAccessor.FastCall(object instance, params object[] parameters)
        {
            if (parameters == null)
            {
                DoCall((T)instance);
                return;
            }
            switch (parameters.Length)
            {
                default:
                    DoCall((T)instance);
                    return;
                case 1:
                    DoCall((T)instance, parameters[0]);
                    return;
                case 2:
                    DoCall((T)instance, parameters[0], parameters[1]);
                    return;
                case 3:
                    DoCall((T)instance, parameters[0], parameters[1], parameters[2]);
                    return;
                case 4:
                    DoCall((T)instance, parameters[0], parameters[1], parameters[2], parameters[3]);
                    return;
                case 5:
                    DoCall((T)instance, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4]);
                    return;
                case 6:
                    DoCall((T)instance, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                    return;
                case 7:
                    DoCall((T)instance, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6]);
                    return;
                case 8:
                    DoCall((T)instance, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7]);
                    return;
            }
        }

        #endregion

        public ActionAccessorBase(string name, params Type[] parTypes)
            : base(name, parTypes)
        {
        }

        protected virtual void DoCall(T instance)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual void DoCall(T instance, object p1)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual void DoCall(T instance, object p1, object p2)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual void DoCall(T instance, object p1, object p2, object p3)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual void DoCall(T instance, object p1, object p2, object p3, object p4)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual void DoCall(T instance, object p1, object p2, object p3, object p4, object p5)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual void DoCall(T instance, object p1, object p2, object p3, object p4, object p5, object p6)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual void DoCall(T instance, object p1, object p2, object p3, object p4, object p5, object p6, object p7)
        {
            throw new InvalidOperationException("Invlaid method call");
        }

        protected virtual void DoCall(T instance, object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8)
        {
            throw new InvalidOperationException("Invlaid method call");
        }
    }

    public sealed class ActionAccessor<T> : ActionAccessorBase<T>
    {
        public ActionAccessor(string name)
            : base(name)
        {
        }

        public void Call(T instance)
        {
            Action<T> call;
            try
            {
                call = (Action<T>)Accessor;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(GetMethodNotFoundErrorMessage(null), ex);
            }
            call(instance);
        }

        protected override void DoCall(T instance)
        {
            Call(instance);
        }
    }

    public sealed class ActionAccessor<T, TP1> : ActionAccessorBase<T>
    {
        public ActionAccessor(string name)
            : base(name, typeof(TP1))
        {
        }

        public void Call(T instance, TP1 p1)
        {
            Action<T, TP1> call = (Action<T, TP1>)Accessor;
            call(instance, p1);
        }

        protected override void DoCall(T instance, object p1)
        {
            Call(instance, (TP1)p1);
        }
    }

    public sealed class ActionAccessor<T, TP1, TP2> : ActionAccessorBase<T>
    {
        public ActionAccessor(string name)
            : base(name, typeof(TP1), typeof(TP2))
        {
        }

        public void Call(T instance, TP1 p1, TP2 p2)
        {
            Action<T, TP1, TP2> call = (Action<T, TP1, TP2>)Accessor;
            call(instance, p1, p2);
        }

        protected override void DoCall(T instance, object p1, object p2)
        {
            Call(instance, (TP1)p1, (TP2)p2);
        }
    }

    public sealed class ActionAccessor<T, TP1, TP2, TP3> : ActionAccessorBase<T>
    {
        public ActionAccessor(string name)
            : base(name, typeof(TP1), typeof(TP2), typeof(TP3))
        {
        }

        public void Call(T instance, TP1 p1, TP2 p2, TP3 p3)
        {
            Action<T, TP1, TP2, TP3> call = (Action<T, TP1, TP2, TP3>)Accessor;
            call(instance, p1, p2, p3);
        }

        protected override void DoCall(T instance, object p1, object p2, object p3)
        {
            Call(instance, (TP1)p1, (TP2)p2, (TP3)p3);
        }
    }

    public sealed class ActionAccessor<T, TP1, TP2, TP3, TP4> : ActionAccessorBase<T>
    {
        public ActionAccessor(string name)
            : base(name, typeof(TP1), typeof(TP2), typeof(TP3), typeof(TP4))
        {
        }

        public void Call(T instance, TP1 p1, TP2 p2, TP3 p3, TP4 p4)
        {
            Action<T, TP1, TP2, TP3, TP4> call = (Action<T, TP1, TP2, TP3, TP4>)Accessor;
            call(instance, p1, p2, p3, p4);
        }

        protected override void DoCall(T instance, object p1, object p2, object p3, object p4)
        {
            Call(instance, (TP1)p1, (TP2)p2, (TP3)p3, (TP4)p4);
        }
    }

    public sealed class ActionAccessor<T, TP1, TP2, TP3, TP4, TP5> : ActionAccessorBase<T>
    {
        public ActionAccessor(string name)
            : base(name, typeof(TP1), typeof(TP2), typeof(TP3), typeof(TP4), typeof(TP5))
        {
        }

        public void Call(T instance, TP1 p1, TP2 p2, TP3 p3, TP4 p4, TP5 p5)
        {
            Action<T, TP1, TP2, TP3, TP4, TP5> call = (Action<T, TP1, TP2, TP3, TP4, TP5>)Accessor;
            call(instance, p1, p2, p3, p4, p5);
        }

        protected override void DoCall(T instance, object p1, object p2, object p3, object p4, object p5)
        {
            Call(instance, (TP1)p1, (TP2)p2, (TP3)p3, (TP4)p4, (TP5) p5);
        }
    }

    public sealed class ActionAccessor<T, TP1, TP2, TP3, TP4, TP5, TP6> : ActionAccessorBase<T>
    {
        public ActionAccessor(string name)
            : base(name, typeof(TP1), typeof(TP2), typeof(TP3), typeof(TP4), typeof(TP5), typeof(TP6))
        {
        }

        public void Call(T instance, TP1 p1, TP2 p2, TP3 p3, TP4 p4, TP5 p5, TP6 p6)
        {
            Action<T, TP1, TP2, TP3, TP4, TP5, TP6> call = (Action<T, TP1, TP2, TP3, TP4, TP5, TP6>)Accessor;
            call(instance, p1, p2, p3, p4, p5, p6);
        }

        protected override void DoCall(T instance, object p1, object p2, object p3, object p4, object p5, object p6)
        {
            Call(instance, (TP1)p1, (TP2)p2, (TP3)p3, (TP4)p4, (TP5)p5, (TP6)p6);
        }
    }

    public sealed class ActionAccessor<T, TP1, TP2, TP3, TP4, TP5, TP6, TP7> : ActionAccessorBase<T>
    {
        public ActionAccessor(string name)
            : base(name, typeof(TP1), typeof(TP2), typeof(TP3), typeof(TP4), typeof(TP5), typeof(TP6), typeof(TP7))
        {
        }

        public void Call(T instance, TP1 p1, TP2 p2, TP3 p3, TP4 p4, TP5 p5, TP6 p6, TP7 p7)
        {
            Action<T, TP1, TP2, TP3, TP4, TP5, TP6, TP7> call = (Action<T, TP1, TP2, TP3, TP4, TP5, TP6, TP7>)Accessor;
            call(instance, p1, p2, p3, p4, p5, p6, p7);
        }

        protected override void DoCall(T instance, object p1, object p2, object p3, object p4, object p5, object p6, object p7)
        {
            Call(instance, (TP1)p1, (TP2)p2, (TP3)p3, (TP4)p4, (TP5)p5, (TP6)p6, (TP7) p7);
        }
    }

    public sealed class ActionAccessor<T, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8> : ActionAccessorBase<T>
    {
        public ActionAccessor(string name)
            : base(name, typeof(TP1), typeof(TP2), typeof(TP3), typeof(TP4), typeof(TP5), typeof(TP6), typeof(TP7), typeof(TP8))
        {
        }

        public void Call(T instance, TP1 p1, TP2 p2, TP3 p3, TP4 p4, TP5 p5, TP6 p6, TP7 p7, TP8 p8)
        {
            Action<T, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8> call  = (Action<T, TP1, TP2, TP3, TP4, TP5, TP6, TP7, TP8>)Accessor;
            call(instance, p1, p2, p3, p4, p5, p6, p7, p8);
        }

        protected override void DoCall(T instance, object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8)
        {
            Call(instance, (TP1)p1, (TP2)p2, (TP3)p3, (TP4)p4, (TP5)p5, (TP6)p6, (TP7)p7, (TP8) p8);
        }
    }
}
