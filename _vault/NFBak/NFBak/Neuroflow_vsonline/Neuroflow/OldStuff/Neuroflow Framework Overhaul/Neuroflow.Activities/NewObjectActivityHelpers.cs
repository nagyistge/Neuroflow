using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Activities;
using System.ComponentModel.DataAnnotations;
using Neuroflow.Core;
using System.Collections;
using System.Collections.ObjectModel;

namespace Neuroflow.Activities
{
    internal class ReflectedItem
    {
        internal bool IsProperty { get; set; }
        internal bool CanRead { get; set; }
        internal bool CanWrite { get; set; }
        internal string Name { get; set; }
        internal Type Type { get; set; }
        internal Attribute[] Attributes { get; set; }
    }

    internal static class NewObjectActivityHelpers
    {
        internal static IEnumerable<ReflectedItem> GetReflectedItems(Type objectType)
        {
            var ci = GetFirstConstructor(objectType);

            var q = from par in ci.GetParameters()
                    select new ReflectedItem
                    {
                        IsProperty = false,
                        CanRead = true,
                        CanWrite = true,
                        Name = par.Name,
                        Type = par.ParameterType,
                        Attributes = GetAttributes(par)
                    };

            q = q.Concat(
                from prop in objectType.GetProperties()
                select new ReflectedItem
                {
                    IsProperty = true,
                    CanRead = prop.CanRead(),
                    CanWrite = prop.CanWrite(),
                    Name = prop.Name,
                    Type = prop.PropertyType,
                    Attributes = GetAttributes(prop)
                });

            return q;
        }

        internal static bool CanRead(this PropertyInfo pi)
        {
            return pi.CanRead && pi.GetGetMethod() != null;
        }

        internal static bool CanWrite(this PropertyInfo pi)
        {
            return pi.CanWrite && pi.GetSetMethod() != null;
        }

        internal static ICustomAttributeProvider GetAttributeProvider(Type objectType, PropertyValue value)
        {
            if (value.ForProperty)
            {
                return objectType.GetProperty(value.Name);
            }
            else
            {
                var ci = GetFirstConstructor(objectType);
                return ci.GetParameters().First(p => p.Name == value.Name);
            }
        }

        internal static ConstructorInfo GetFirstConstructor(Type type)
        {
            var foundCI = type.GetConstructors().OrderBy(ci => ci.GetParameters().Length).FirstOrDefault();
            return foundCI;
        }

        internal static Attribute[] MakeActivityWithResultCollectionAttributes(Attribute[] attributes)
        {
            var list = new LinkedList<Attribute>(attributes);
            list.AddLast(new BrowsableAttribute(false));
            return list.ToArray();
        }

        internal static void AddChildActivityWithResult(
            NativeActivityMetadata metadata,
            ActivityWithResult activity,
            Type reqActivityResultType,
            string properyName,
            Attribute[] attributes)
        {
            // Validate:
            bool isReq = IsRequired(attributes);
            if (isReq && activity == null) metadata.AddValidationError(properyName + " is missing.");
            if (activity != null)
            {
                var activityResultType = activity.ResultType;
                if (activityResultType != reqActivityResultType && !activityResultType.IsSubclassOf(reqActivityResultType))
                {
                    metadata.AddValidationError(properyName + " result type is incompatible.");
                }
            }

            metadata.AddChild(activity);
        }

        internal static Attribute[] GetAttributes(ICustomAttributeProvider provider)
        {
            var q = provider.GetCustomAttributes(true).Cast<Attribute>();
            var list = new LinkedList<Attribute>(q);

            if (provider is ParameterInfo && !list.Any(a => a.GetType() == typeof(CategoryAttribute)))
            {
                list.AddLast(new CategoryAttribute("Constructor"));
            }

            return list.ToArray();
        }

        internal static Type GetCollectionItemType(Type type)
        {
            foreach (var itype in type.GetInterfaces())
            {
                if (itype.IsGenericType && itype.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return itype.GetGenericArguments()[0];
                }
            }
            return null;
        }

        internal static RuntimeArgument CreateRuntimeArgument(string propName, Type propType, Attribute[] attributes)
        {
            return new RuntimeArgument(
                    propName,
                    propType,
                    ArgumentDirection.In,
                    IsRequired(attributes));
        }

        internal static bool IsRequired(Attribute[] attributes)
        {
            return attributes.Select(a => a.GetType()).Any(t => t == typeof(RequiredArgumentAttribute) || t == typeof(RequiredAttribute));
        }

        internal static Type GetActivityWithResultType(Type type)
        {
            return typeof(Activity<>).MakeGenericType(type);
        }

        internal static Type GetActivityWithResultCollectionType(Type type)
        {
            return typeof(Activity<>).MakeGenericType(GetCollectionType(type));
        }

        internal static Type GetCollectionType(Type type)
        {
            return typeof(Collection<>).MakeGenericType(type);
        }

        internal static void SetPropValue(object obj, string properyName, object value)
        {
            GetPropInfo(obj, properyName).SetValue(obj, value, null);
        }

        internal static void SetCollectionPropValue(object obj, string properyName, IEnumerable<object> items)
        {
            var pi = GetPropInfo(obj, properyName);
            if (pi.CanWrite())
            {
                SetWriteableCollPropValue(obj, pi, items);
            }
            else
            {
                SetReadOnlyCollPropValue(obj, pi, items);
            }
        }

        private static void SetReadOnlyCollPropValue(object obj, PropertyInfo pi, IEnumerable<object> items)
        {
            var coll = pi.GetValue(obj, null);
            if (coll != null)
            {
                AddItemsToCollection(coll, items);
            }
        }

        private static void SetWriteableCollPropValue(object obj, PropertyInfo pi, IEnumerable<object> items)
        {
            pi.SetValue(obj, ConvertTo(items, pi.PropertyType), null);
        }

        private static void AddItemsToCollection(object collection, IEnumerable<object> items)
        {
            var add = GetAddMethod(collection.GetType());
            if (add != null)
            {
                var args = new object[1];
                foreach (var item in items)
                {
                    args[0] = item;
                    add.Invoke(collection, args);
                }
            }
        }

        private static MethodInfo GetAddMethod(Type collectionType)
        {
            return collectionType.GetMethod("Add");
        }

        private static PropertyInfo GetPropInfo(object obj, string properyName)
        {
            return obj.GetType().GetProperty(properyName);
        }

        internal static MethodInfo GetGenericMethod(object obj, Type typeArg, string methodName)
        {
            var mi = obj.GetType().GetMethod(methodName);
            var gmi = mi.MakeGenericMethod(typeArg);
            return gmi;
        }

        internal static object ConvertTo(object value, Type type)
        {
            if (value != null)
            {
                var valueType = value.GetType();

                if (!valueType.IsValueType &&
                    valueType != typeof(string) &&
                    valueType.Implements<IEnumerable>() &&
                    !type.IsValueType &&
                    type != typeof(string) &&
                    type.Implements<IEnumerable>())
                {
                    if (type.IsArray)
                    {
                        var itemType = type.GetElementType();
                        var collType = typeof(List<>).MakeGenericType(itemType);
                        var resultColl = (IList)Activator.CreateInstance(collType);
                        foreach (var item in (IEnumerable)value)
                        {
                            resultColl.Add(item);
                        }
                        var toArrayMethod = collType.GetMethod("ToArray");
                        return toArrayMethod.Invoke(resultColl, null);
                    }
                    else
                    {
                        var resultColl = (IList)Activator.CreateInstance(type);
                        foreach (var item in (IEnumerable)value)
                        {
                            resultColl.Add(item);
                        }
                        return resultColl;
                    }
                }
            }
            return value;
        }

        #region Create Object

        internal static T CreateObject<T>(Type objectType, IEnumerable<PropertyValue> propValues, Dictionary<string, object> createdValues)
        {
            var obj = ConstructObject<T>(objectType, createdValues);
            SetProperties<T>(obj, propValues, createdValues);
            return obj;
        }

        private static T ConstructObject<T>(Type objectType, Dictionary<string, object> createdValues)
        {
            var ci = NewObjectActivityHelpers.GetFirstConstructor(objectType);
            var pars = ci.GetParameters();
            var parObjects = new object[pars.Length];

            for (int idx = 0; idx < parObjects.Length; idx++)
            {
                object value = GetValue(pars[idx].Name, createdValues);
                parObjects[idx] = ConvertTo(value, pars[idx].ParameterType);
            }

            return (T)ci.Invoke(parObjects);
        }

        private static void SetProperties<T>(T obj, IEnumerable<PropertyValue> values, Dictionary<string, object> createdValues)
        {
            foreach (var value in values)
            {
                var inArgValue = value as InArgumentValue;
                if (inArgValue != null)
                {
                    SetProperty<T>(obj, inArgValue, createdValues);
                    continue;
                }
                var activityValue = value as ActivityWithResultValue;
                if (activityValue != null)
                {
                    SetProperty<T>(obj, activityValue, createdValues);
                    continue;
                }
            }
        }

        private static void SetProperty<T>(T obj, PropertyValue propValue, Dictionary<string, object> createdValues)
        {
            object value;
            if (TryGetValue(propValue.Name, createdValues, out value))
            {
                NewObjectActivityHelpers.SetPropValue(obj, propValue.Name, value);
            }
        }

        private static object GetValue(string name, Dictionary<string, object> createdValues)
        {
            object value;
            if (createdValues.TryGetValue(name, out value)) return value;
            return null;
        }

        private static bool TryGetValue(string name, Dictionary<string, object> createdValues, out object value)
        {
            return createdValues.TryGetValue(name, out value);
        }

        #endregion
    }
}
