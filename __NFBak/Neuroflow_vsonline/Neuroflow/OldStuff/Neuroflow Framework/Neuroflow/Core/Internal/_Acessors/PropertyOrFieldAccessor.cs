using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Neuroflow.Core.Internal
{
    public interface IPropertyOrFieldAccessor : IAccessor
    {
        object Get(object instance);

        void Set(object instance, object value);

        object FastGet(object instance);

        void FastSet(object instance, object value);

        bool CanGet { get; }

        bool CanSet { get; }

        Type PropertyOrFieldType { get; }

        PropertyInfo PropertyInfo { get; }

        FieldInfo FieldInfo { get; }
    }
    
    public sealed class PropertyOrFieldAccessor<T, TPropOrField> : Accessor<T>, IPropertyOrFieldAccessor
    {
        delegate TPropOrField GetAccessor(T instance);

        delegate void SetAccessor(T instance, TPropOrField value);

        #region IPropertyOrFieldAccessor Members

        public object Get(object instance)
        {
            if (instance != null && !IsInstanceTypeCorrect(instance)) throw new ArgumentException(GetInstanceTypeErrorMessage(), "instance");
            return this.Get((T)instance);
        }

        public void Set(object instance, object value)
        {
            if (instance != null && !IsInstanceTypeCorrect(instance)) throw new ArgumentException(GetInstanceTypeErrorMessage(), "instance");
            if (!IsObjectOkForType(value, typeof(TPropOrField))) throw new ArgumentException(GetValueTypeErrorMessage(), "value");
            this.Set((T)instance, (TPropOrField)value);
        }

        public object FastGet(object instance)
        {
            return this.Get((T)instance);
        }

        public void FastSet(object instance, object value)
        {
            this.Set((T)instance, (TPropOrField)value);
        }

        public PropertyInfo PropertyInfo
        {
            get { return this.MemberInfo as PropertyInfo; }
        }

        public FieldInfo FieldInfo
        {
            get { return this.MemberInfo as FieldInfo; }
        }

        public Type PropertyOrFieldType
        {
            get { return typeof(TPropOrField); }
        }

        public bool CanGet
        {
            get { return getAccessor != null; }
        }

        public bool CanSet
        {
            get { return setAccessor != null; }
        }

        #endregion

        public PropertyOrFieldAccessor(string name)
            : base(name)
        {
            CreateSetAccessor();
            CreateGetAccessor();
            if (getAccessor == null && setAccessor == null)
            {
                throw new ArgumentException(string.Format("Can not access property '{0}'.", name), name);
            }
        }

        private void CreateGetAccessor()
        {
            PropertyInfo pi = PropertyInfo;
            if (pi != null)
            {
                if (pi.CanRead)
                {
                    var pInstance = GetInstanceParameter();
                    var expr = Expression.Property(pInstance, pi);
                    try
                    {
                        getAccessor = Expression.Lambda<GetAccessor>(expr, pInstance).Compile();
                    }
                    catch
                    {
                        // unreadable
                    }
                }
            }
            else
            {
                FieldInfo fi = FieldInfo;
                var pInstance = GetInstanceParameter();
                var expr = Expression.Field(pInstance, fi);
                try
                {
                    getAccessor = Expression.Lambda<GetAccessor>(expr, pInstance).Compile();
                }
                catch
                {
                    // unreadable
                }
            }
        }

        private void CreateSetAccessor()
        {
            var propInfo = GetPropertyInfo();
            if (propInfo != null)
            {
                this.MemberInfo = propInfo;
                if (propInfo.CanWrite)
                {
                    var pInstance = GetInstanceParameter();
                    var pValue = Expression.Parameter(typeof(TPropOrField), "value");
                    var eSet = MethodCallExpression.Call(pInstance, propInfo.GetSetMethod(true), pValue);
                    try
                    {
                        setAccessor = Expression.Lambda<SetAccessor>(eSet, pInstance, pValue).Compile();
                    }
                    catch
                    {
                        // unwriteable
                    }
                }
            }
            else
            {
                FieldInfo setField;
                Type fieldType = typeof(TPropOrField);
                Type objectType = typeof(T);

                setField = objectType.GetField(Name, BindingFlags.Public | BindingFlags.Instance);
                if (setField == null)
                {
                    setField = objectType.GetField(Name, BindingFlags.Public | BindingFlags.Static);
                }
                if (setField == null)
                {
                    try { setField = objectType.GetField(Name, BindingFlags.NonPublic | BindingFlags.Instance); }
                    catch { };
                }
                if (setField == null)
                {
                    try { setField = objectType.GetField(Name, BindingFlags.NonPublic | BindingFlags.Static); }
                    catch { };
                }
                if (setField != null)
                {
                    this.MemberInfo = setField;
                    if (setField.FieldType != fieldType) throw new InvalidOperationException("Invalid field type.");
                    try
                    {
                        DynamicMethod dm = new DynamicMethod(string.Format("__setField__{0}", setField.Name),
                            null,
                            new Type[] { objectType, fieldType },
                            objectType);

                        ILGenerator il = dm.GetILGenerator();
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Stfld, setField);
                        il.Emit(OpCodes.Ret);

                        setAccessor = (SetAccessor)dm.CreateDelegate(typeof(SetAccessor));
                    }
                    catch
                    {
                        // unwriteable
                    }
                }
            }
        }

        private PropertyInfo GetPropertyInfo()
        {
            PropertyInfo pi = typeof(T).GetProperty(Name);
            if (pi == null)
            {
                try { pi = typeof(T).GetProperty(Name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.ExactBinding); }
                catch { };
            }
            if (pi == null)
            {
                try { pi = typeof(T).GetProperty(Name, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.ExactBinding); }
                catch { };
            }
            return pi;
        }

        GetAccessor getAccessor;

        SetAccessor setAccessor;

        public TPropOrField Get(T instance)
        {
            if (getAccessor == null) throw new InvalidOperationException("Get accessor not present.");
            return getAccessor(instance);
        }

        public void Set(T instance, TPropOrField value)
        {
            if (setAccessor != null)
            {
                setAccessor(instance, value);
                return;
            }
            throw new InvalidOperationException("Set accessor not present.");
        }

        private static string GetValueTypeErrorMessage()
        {
            return string.Format("Value argument type is not '{0}'.", typeof(TPropOrField).FullName);
        }

        private bool IsInstanceTypeCorrect(object instance)
        {
            Type type = instance.GetType();
            return type == typeof(T) || (type.IsClass && type.IsSubclassOf(typeof(T)));
        }
    }
}
