using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.Internal
{
    internal static class TypeExtensions
    {
        internal static bool Implements<T>(this Type type)
        {
            Contract.Requires(type != null);

            return ImplementsCheck(type, typeof(T));
        }
        
        internal static bool Implements(this Type type, Type interfaceType)
        {
            Contract.Requires(type != null);
            Contract.Requires(interfaceType != null);

            return ImplementsCheck(type, interfaceType);
        }

        private static bool ImplementsCheck(Type type, Type interfaceType)
        {
            if (!interfaceType.IsInterface) return false;
            if (type == interfaceType) return true;
            return type.GetInterfaces().Any(it => it == interfaceType);
        }

        internal static bool IsA<T>(this Type type)
        {
            Contract.Requires(type != null);

            return IsACheck(type, typeof(T));
        }

        internal static bool IsA(this Type type1, Type type2)
        {
            Contract.Requires(type1 != null);
            Contract.Requires(type2 != null);

            return IsACheck(type1, type2);
        }

        private static bool IsACheck(Type type1, Type type2)
        {
            if (type1 == type2) return true;
            if (type2.IsInterface) return ImplementsCheck(type1, type2);
            if (type1.IsClass && type2.IsClass) return type1.IsSubclassOf(type2);
            return false;
        }

        internal static bool IsNullable(this Type type)
        {
            Contract.Requires(type != null);

            return type.IsValueType && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
