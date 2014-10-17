using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Core
{
    #region Interface

    [ContractClass(typeof(IFactoryContract))]
    public interface IFactory
    {
        object Create();

        Type ObjectType { get; }
    }

    [ContractClassFor(typeof(IFactory))]
    class IFactoryContract : IFactory
    {
        object IFactory.Create()
        {
            Contract.Ensures(Contract.Result<object>() != null);
            return null;
        }

        Type IFactory.ObjectType
        {
            get { return null; }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(((IFactory)this).ObjectType != null);
        }

    }

    [ContractClass(typeof(IFactoryContract<>))]
    public interface IFactory<out T> : IFactory
    {
        new T Create();
    }

    [ContractClassFor(typeof(IFactory<>))]
    class IFactoryContract<T> : IFactory<T>
    {
        T IFactory<T>.Create()
        {
            Contract.Ensures(Contract.Result<T>() != null);
            return default(T);
        }

        object IFactory.Create()
        {
            throw new NotImplementedException();
        }

        Type IFactory.ObjectType
        {
            get { throw new NotImplementedException(); }
        }
    }

    #endregion

    #region Implementation

    public sealed class Factory<T> : IFactory<T>
    {
        enum Mode : byte { Singleton, Method, Constructor }

        #region Constructors

        public Factory(params object[] constructorParameters)
        {
            mode = Mode.Constructor;
            this.constructorParameters = constructorParameters;
        }

        public Factory(T singleton)
        {
            Contract.Requires(!object.ReferenceEquals(singleton, null));
            
            mode = Mode.Singleton;
            this.singleton = singleton;
        }

        public Factory(Func<T> factoryMethod)
        {
            Contract.Requires(factoryMethod != null);
            
            mode = Mode.Method;
            this.factoryMethod = factoryMethod;
        }

        public Factory(IFactory<T> factory)
        {
            Contract.Requires(factory != null);

            mode = Mode.Method;
            factoryMethod = () => factory.Create();
        }

        public Factory(Factory<T> factory)
        {
            Contract.Requires(factory != null);

            switch (mode = factory.mode)
            {
                case Mode.Constructor:
                    constructorParameters = factory.constructorParameters;
                    break;
                case Mode.Method:
                    factoryMethod = factory.factoryMethod;
                    break;
                case Mode.Singleton:
                    singleton = factory.singleton;
                    break;
            }
        }

        #endregion

        #region Fields

        Mode mode;

        T singleton;

        Func<T> factoryMethod;

        object[] constructorParameters; 

        #endregion

        #region Create

        public T Create()
        {
            switch (mode)
            {
                case Mode.Method:
                    return factoryMethod();
                case Mode.Constructor:
                    if (constructorParameters.IsNullOrEmpty())
                    {
                        return Activator.CreateInstance<T>();
                    }
                    else
                    {
                        return (T)Activator.CreateInstance(typeof(T), constructorParameters);
                    }
                default:
                    return singleton;
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

        #endregion

        #region Implicit Operators

        public static implicit operator Factory<T>(T singleton)
        {
            return new Factory<T>(singleton);
        }

        public static implicit operator Factory<T>(Func<T> factoryMethod)
        {
            Contract.Requires(factoryMethod != null);

            return new Factory<T>(factoryMethod);
        }

        #endregion
    } 

    #endregion

    #region Extension

    public static class FactoryExtensions
    {
        struct FakeFactory<T> : IFactory<T> where T : class
        {
            internal FakeFactory(T obj)
            {
                Contract.Requires(obj != null);

                this.obj = obj;
            }

            T obj;

            T IFactory<T>.Create()
            {
                return obj;
            }

            object IFactory.Create()
            {
                return obj;
            }

            Type IFactory.ObjectType
            {
                get { return typeof(T); }
            }
        }
        
        public static IFactory<T> AsFactory<T>(this T obj) where T : class
        {
            Contract.Requires(obj != null);

            return new FakeFactory<T>(obj);
        }
    }

    #endregion
}
