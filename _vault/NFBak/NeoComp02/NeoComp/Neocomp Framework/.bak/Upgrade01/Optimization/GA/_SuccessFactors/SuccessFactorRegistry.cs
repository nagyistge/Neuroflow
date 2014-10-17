using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using NeoComp.Core;
using System.Collections;
using System.Linq.Expressions;

namespace NeoComp.Optimization.GA
{
    internal static class SuccessFactorRegistry
    {
        #region Container

        static readonly Dictionary<Type, SuccessFactorContainerAttribute> containerDict = new Dictionary<Type, SuccessFactorContainerAttribute>();

        internal static SuccessFactorContainerAttribute GetSuccessFactorContainerAttribute(Type type)
        {
            Debug.Assert(type != null);
            Debug.Assert(type.IsClass);
            SuccessFactorContainerAttribute attrib;

            lock (containerDict)
            {
                if (containerDict.TryGetValue(type, out attrib)) return attrib;
                attrib = type.GetCustomAttributes(typeof(SuccessFactorContainerAttribute), true).Cast<SuccessFactorContainerAttribute>().FirstOrDefault();
                containerDict.Add(type, attrib);
                return attrib;
            }
        }

        internal static bool IsSuccessFactorContainer(Type type)
        {
            Debug.Assert(type != null);
            if (type.IsClass) return GetSuccessFactorContainerAttribute(type) != null;
            return false;
        }

        #endregion

        #region Creator

        #region Creator Class

        internal interface ISuccessFactorCreator
        {
            ISuccessFactor Create(object container);
        }

        internal sealed class SuccessFactorCreator<TContainer, TProperty> : ISuccessFactorCreator
            where TContainer : class
        {
            public SuccessFactorCreator(PropertyInfo propertyInfo, SuccessFactorAttribute attribute)
            {
                Debug.Assert(propertyInfo != null);
                Debug.Assert(attribute != null);
                Debug.Assert(propertyInfo.PropertyType == typeof(TProperty));

                this.attribute = attribute;
                this.accessor = new PropertyOrFieldAccessor<TContainer, TProperty>(propertyInfo.Name);
                Initialize(propertyInfo);
            }

            #region Fields

            PropertyOrFieldAccessor<TContainer, TProperty> accessor;

            SuccessFactorAttribute attribute;

            Func<TContainer, PropertyOrFieldAccessor<TContainer, TProperty>, ComparationMode, SuccessFactor<TContainer>> createFunc;

            #endregion

            #region Create

            internal SuccessFactor<TContainer> Create(TContainer container)
            {
                return createFunc(container, accessor, attribute.ComparationMode);
            }

            #endregion

            #region Initialize

            private void Initialize(PropertyInfo propertyInfo)
            {
                var containerType = propertyInfo.ReflectedType;
                if (IsSuccessFactorContainer(propertyInfo.PropertyType))
                {
                    var factorType = typeof(ObjectPropertySuccessFactor<,>).MakeGenericType(containerType, accessor.PropertyOrFieldType);
                    createFunc = Compile(containerType, accessor, factorType);
                }
                else if (propertyInfo.PropertyType.Implements<IComparable>())
                {
                    var factorType = typeof(PropertySuccessFactor<,>).MakeGenericType(containerType, accessor.PropertyOrFieldType);
                    createFunc = Compile(containerType, accessor, factorType);
                }
                else if (propertyInfo.PropertyType.Implements<IEnumerable>())
                {
                    var factorType = typeof(CollectionPropertySuccessFactor<,,>).MakeGenericType(containerType, accessor.PropertyOrFieldType, GetCollectionItemType(accessor.PropertyOrFieldType));
                    createFunc = Compile(containerType, accessor, factorType);
                }
                else
                {
                    // TODO: Add error message here.
                    throw new InvalidOperationException("TODO: Add error message here.");
                }
            }

            private static Type GetCollectionItemType(Type collectionType)
            {
                var q = from iType in collectionType.GetInterfaces()
                        where iType.IsGenericType
                        let def = iType.GetGenericTypeDefinition()
                        where def == typeof(IEnumerable<>)
                        select iType.GetGenericArguments()[0];
                var itemType = q.FirstOrDefault();
                if (itemType == null)
                {
                    throw new InvalidOperationException("Succes factor collection property of type " + collectionType + " is not a generic IEnumerable type.");
                }
                if (!itemType.IsClass)
                {
                    throw new InvalidOperationException("Succes factor collection property of type " + collectionType + "'s item type is not reference type.");
                }
                return itemType;
            }

            private Func<TContainer, PropertyOrFieldAccessor<TContainer, TProperty>, ComparationMode, SuccessFactor<TContainer>> Compile(Type containerType, IPropertyOrFieldAccessor accessor, Type factorType)
            {
                var constructorInfo = factorType.GetConstructor(new[] { containerType, accessor.GetType(), typeof(ComparationMode) });
                var containerArgExpr = Expression.Parameter(containerType, "container");
                var accessorArgExpr = Expression.Parameter(accessor.GetType(), "accessor");
                var modeArgExpr = Expression.Parameter(typeof(ComparationMode), "mode");
                Expression createExpr = Expression.New(constructorInfo, containerArgExpr, accessorArgExpr, modeArgExpr);
                return Expression.Lambda<Func<TContainer, PropertyOrFieldAccessor<TContainer, TProperty>, ComparationMode, SuccessFactor<TContainer>>>(createExpr, containerArgExpr, accessorArgExpr, modeArgExpr).Compile();
            }

            #endregion

            #region ISuccessFactorCreator Members

            ISuccessFactor ISuccessFactorCreator.Create(object container)
            {
                Debug.Assert(container != null);
                Debug.Assert(container is TContainer);
                return Create((TContainer)container);
            }

            #endregion
        } 

        #endregion

        static readonly Dictionary<Type, ISuccessFactorCreator[]> creatorDict = new Dictionary<Type, ISuccessFactorCreator[]>();

        internal static ISuccessFactorCreator[] GetCreators(Type type)
        {
            Debug.Assert(type != null);
            Debug.Assert(type.IsClass);

            lock (creatorDict)
            {
                ISuccessFactorCreator[] creators;
                if (creatorDict.TryGetValue(type, out creators)) return creators;
                creators = CreateSuccessFactorCreators(type);
                creatorDict.Add(type, creators);
                return creators;
            }
        }

        private static ISuccessFactorCreator[] CreateSuccessFactorCreators(Type type)
        {
            return (from pi in type.GetProperties()
                    where pi.CanRead
                    let attrib = pi.GetCustomAttributes(typeof(SuccessFactorAttribute), true).Cast<SuccessFactorAttribute>().FirstOrDefault()
                    where attrib != null
                    orderby attrib.Order
                    let creatorType = typeof(SuccessFactorCreator<,>).MakeGenericType(type, pi.PropertyType)
                    select (ISuccessFactorCreator)Activator.CreateInstance(creatorType, pi, attrib)).ToArray();
        }

        #endregion

        #region Create Func

        static Dictionary<Type, Delegate> createFuncDict = new Dictionary<Type, Delegate>();

        internal static Delegate GetCreateFunc(Type objType)
        {
            Debug.Assert(objType != null);
            Debug.Assert(objType.IsClass);
            lock (createFuncDict)
            {
                Delegate func;
                if (!createFuncDict.TryGetValue(objType, out func))
                {
                    func = CompileCreateFunc(objType);
                    createFuncDict.Add(objType, func);
                }
                return func;
            }
        }

        private static Delegate CompileCreateFunc(Type objType)
        {
            var mi = typeof(SuccessFactorFactory).GetMethod("Create");
            Debug.Assert(mi != null);
            mi = mi.MakeGenericMethod(new[] { objType });
            Debug.Assert(mi != null);

            var objPar = Expression.Parameter(objType, "obj");
            var callExp = Expression.Call(mi, objPar);
            var func = Expression.Lambda(callExp, objPar).Compile();

            return func;
        }

        #endregion
    }
}
