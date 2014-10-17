using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Core
{
    internal static class TypeExtensions
    {
        public static bool Implements<T>(this Type type)
        {
            Args.IsNotNull(type, "type");
            return ImplementsCheck(type, typeof(T));
        }
        
        public static bool Implements(this Type type, Type interfaceType)
        {
            Args.AreNotNull(type, "type", interfaceType, "interfaceType");
            return ImplementsCheck(type, interfaceType);
        }

        private static bool ImplementsCheck(Type type, Type interfaceType)
        {
            if (!interfaceType.IsInterface) throw new ArgumentException("Interface type expected.", "interfaceType"); 
            if (type == interfaceType) return true;
            return type.GetInterfaces().Any(it => it == interfaceType);
        }

        public static bool IsA<T>(this Type type)
        {
            Args.IsNotNull(type, "type");
            return IsACheck(type, typeof(T));
        }

        public static bool IsA(this Type type1, Type type2)
        {
            Args.AreNotNull(type1, "type1", type2, "type2");
            return IsACheck(type1, type2);
        }

        private static bool IsACheck(Type type1, Type type2)
        {
            if (type1 == type2) return true;
            if (type2.IsInterface) return ImplementsCheck(type1, type2);
            if (type1.IsClass && type2.IsClass) return type1.IsSubclassOf(type2);
            return false;
        }

        public static bool IsNullable(this Type type)
        {
            Args.IsNotNull(type, "type");
            return type.IsValueType && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
