using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace NeoComp.Internal
{
    public static class AccessorFactory
    {
        #region Property or field accessor creation

        public static IPropertyOrFieldAccessor CreatePropertyOrFieldAccessor(MemberInfo memberInfo)
        {
            if (memberInfo == null) throw new ArgumentNullException("memberInfo");
            if (memberInfo.MemberType != MemberTypes.Field && memberInfo.MemberType != MemberTypes.Property)
            {
                throw new ArgumentException("Invalid member type.", "memberInfo");
            }
            return DoCreatePropertyOrFieldAccessor(memberInfo.DeclaringType, GetMemberType(memberInfo), memberInfo.Name);
        }

        public static IPropertyOrFieldAccessor CreatePropertyOrFieldAccessor(Type type, Type propertyOrFieldType, string name)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (propertyOrFieldType == null) throw new ArgumentNullException("propertyOrFieldType");
            return DoCreatePropertyOrFieldAccessor(type, propertyOrFieldType, name);
        }

        private static IPropertyOrFieldAccessor DoCreatePropertyOrFieldAccessor(Type type, Type propertyOrFieldType, string name)
        {
            Type genericType = typeof(PropertyOrFieldAccessor<,>).MakeGenericType(type, propertyOrFieldType);
            return (IPropertyOrFieldAccessor)Activator.CreateInstance(genericType, name);
        } 

        #endregion

        #region Action accessor creation

        public static IActionAccessor CreateActionAccessor(MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException("methodInfo");
            if (methodInfo.ReturnType != typeof(void)) throw new ArgumentException("Action method expected.", "methodInfo");
            return DoCreateActionAccessor(methodInfo.DeclaringType, methodInfo.Name, methodInfo.GetParameters().Select(pi => pi.ParameterType).ToArray());
        }

        public static IActionAccessor CreateActionAccessor(Type type, string name, params Type[] parTypes)
        {
            if (type == null) throw new ArgumentNullException("type"); 
            return DoCreateActionAccessor(type, name, parTypes);
        }

        private static IActionAccessor DoCreateActionAccessor(Type type, string name, Type[] parTypes)
        {
            if (parTypes == null) parTypes = Type.EmptyTypes;
            Type genericType;
            switch (parTypes.Length)
            {
                case 0:
                    genericType = typeof(ActionAccessor<>).MakeGenericType(type);
                    break;
                case 1:
                    genericType = typeof(ActionAccessor<,>).MakeGenericType(type, parTypes[0]);
                    break;
                case 2:
                    genericType = typeof(ActionAccessor<,,>).MakeGenericType(type, parTypes[0],
                                                                                 parTypes[1]);
                    break;
                case 3:
                    genericType = typeof(ActionAccessor<,,,>).MakeGenericType(type, parTypes[0],
                                                                                 parTypes[1],
                                                                                 parTypes[2]);
                    break;
                case 4:
                    genericType = typeof(ActionAccessor<,,,,>).MakeGenericType(type, parTypes[0],
                                                                                 parTypes[1],
                                                                                 parTypes[2],
                                                                                 parTypes[3]);
                    break;
                case 5:
                    genericType = typeof(ActionAccessor<,,,,,>).MakeGenericType(type, parTypes[0],
                                                                                 parTypes[1],
                                                                                 parTypes[2],
                                                                                 parTypes[3],
                                                                                 parTypes[4]);
                    break;
                case 6:
                    genericType = typeof(ActionAccessor<,,,,,,>).MakeGenericType(type, parTypes[0],
                                                                                 parTypes[1],
                                                                                 parTypes[2],
                                                                                 parTypes[3],
                                                                                 parTypes[4],
                                                                                 parTypes[5]);
                    break;
                case 7:
                    genericType = typeof(ActionAccessor<,,,,,,,>).MakeGenericType(type, parTypes[0],
                                                                                 parTypes[1],
                                                                                 parTypes[2],
                                                                                 parTypes[3],
                                                                                 parTypes[4],
                                                                                 parTypes[5],
                                                                                 parTypes[6]);
                    break;
                case 8:
                    genericType = typeof(ActionAccessor<,,,,,,,,>).MakeGenericType(type, parTypes[0], 
                                                                                 parTypes[1], 
                                                                                 parTypes[2], 
                                                                                 parTypes[3], 
                                                                                 parTypes[4], 
                                                                                 parTypes[5],
                                                                                 parTypes[6], 
                                                                                 parTypes[7]);
                    break;
                default:
                    throw new InvalidOperationException("Too many method parameters.");
            }
            return (IActionAccessor)Activator.CreateInstance(genericType, name);
        }

        #endregion

        #region Func accessor creation

        public static IFuncAccessor CreateFuncAccessor(MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException("methodInfo");
            if (methodInfo.ReturnType == typeof(void)) throw new ArgumentException("Func method expected.", "methodInfo");
            return DoCreateFuncAccessor(methodInfo.DeclaringType, methodInfo.ReturnType, methodInfo.Name, methodInfo.GetParameters().Select(pi => pi.ParameterType).ToArray());
        }

        public static IFuncAccessor CreateFuncAccessor(Type type, Type resultType, string name, params Type[] parTypes)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (resultType == null) throw new ArgumentNullException("resultType");
            return DoCreateFuncAccessor(type, resultType, name, parTypes);
        }

        private static IFuncAccessor DoCreateFuncAccessor(Type type, Type resultType, string name, Type[] parTypes)
        {
            if (parTypes == null) parTypes = Type.EmptyTypes;
            Type genericType;
            switch (parTypes.Length)
            {
                case 0:
                    genericType = typeof(FuncAccessor<,>).MakeGenericType(type, resultType);
                    break;
                case 1:
                    genericType = typeof(FuncAccessor<,,>).MakeGenericType(type, parTypes[0], resultType);
                    break;
                case 2:
                    genericType = typeof(FuncAccessor<,,,>).MakeGenericType(type, parTypes[0],
                                                                                 parTypes[1], resultType);
                    break;
                case 3:
                    genericType = typeof(FuncAccessor<,,,,>).MakeGenericType(type, parTypes[0],
                                                                                 parTypes[1],
                                                                                 parTypes[2], resultType);
                    break;
                case 4:
                    genericType = typeof(FuncAccessor<,,,,,>).MakeGenericType(type, parTypes[0],
                                                                                 parTypes[1],
                                                                                 parTypes[2],
                                                                                 parTypes[3], resultType);
                    break;
                case 5:
                    genericType = typeof(FuncAccessor<,,,,,,>).MakeGenericType(type, parTypes[0],
                                                                                 parTypes[1],
                                                                                 parTypes[2],
                                                                                 parTypes[3],
                                                                                 parTypes[4], resultType);
                    break;
                case 6:
                    genericType = typeof(FuncAccessor<,,,,,,,>).MakeGenericType(type, parTypes[0],
                                                                                 parTypes[1],
                                                                                 parTypes[2],
                                                                                 parTypes[3],
                                                                                 parTypes[4],
                                                                                 parTypes[5], resultType);
                    break;
                case 7:
                    genericType = typeof(FuncAccessor<,,,,,,,,>).MakeGenericType(type, parTypes[0],
                                                                                 parTypes[1],
                                                                                 parTypes[2],
                                                                                 parTypes[3],
                                                                                 parTypes[4],
                                                                                 parTypes[5],
                                                                                 parTypes[6], resultType);
                    break;
                case 8:
                    genericType = typeof(FuncAccessor<,,,,,,,,,>).MakeGenericType(type, parTypes[0],
                                                                                 parTypes[1],
                                                                                 parTypes[2],
                                                                                 parTypes[3],
                                                                                 parTypes[4],
                                                                                 parTypes[5],
                                                                                 parTypes[6],
                                                                                 parTypes[7], resultType);
                    break;
                default:
                    throw new InvalidOperationException("Too many method parameters.");
            }
            return (IFuncAccessor)Activator.CreateInstance(genericType, name);
        }

        #endregion

        private static Type GetMemberType(MemberInfo memberInfo)
        {
            var pi = memberInfo as PropertyInfo;
            if (pi != null) return pi.PropertyType;
            var fi = memberInfo as FieldInfo;
            Debug.Assert(fi != null);
            return fi.FieldType;
        }
    }
}
