using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using NeoComp.Core;
using System.Collections;
using System.Linq.Expressions;

namespace NeoComp.Optimization.GA
{
    public static class SuccessFactorFactory
    {
        #region Container Create

        public static ISuccessFactor CreateFromObject(object obj)
        {
            obj.IsNotNull("obj");
            var objType = obj.GetType();
            if (!objType.IsClass) throw new ArgumentException("Object type is not reference.", "obj");

            return (ISuccessFactor)SuccessFactorRegistry.GetCreateFunc(objType).DynamicInvoke(obj);
        }

        public static ObjectSuccessFactor<T> Create<T>(T obj)
            where T : class
        {
            Args.IsNotNull(obj, "obj");
            var objType = obj.GetType();
            var tType = typeof(T);
            if (objType != tType) throw new InvalidOperationException("Object succes factor covariance is not supported.");
            return CreateInternal<T>(obj);
        }

        internal static ObjectSuccessFactor<T> CreateInternal<T>(T obj) 
            where T : class
        {
            Debug.Assert(obj != null);
            var objType = obj.GetType();
            var attrib = SuccessFactorRegistry.GetSuccessFactorContainerAttribute(objType);
            if (attrib == null) throw new ArgumentException("Object is not a Success Container. SuccessFactorContainerAttribute expected.", "obj");
            return new ObjectSuccessFactor<T>(obj);
        } 

        #endregion

        internal static List<SuccessFactor<T>> GetFactors<T>(T container)
            where T : class
        {
            Debug.Assert(container != null);
            return SuccessFactorRegistry.GetCreators(container.GetType())
                                        .Select(c => (SuccessFactor<T>)c.Create(container))
                                        .ToList();
        }
    }
}
