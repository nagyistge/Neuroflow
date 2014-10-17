using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics.Contracts;

namespace NeoComp.Core
{
    public static class Args
    {
        #region Not null

        public static void IsNotNull(this object arg, string argName)
        {
            CheckIsNotNull(arg, argName);
        }

        public static void AreNotNull(object arg1, string argName1, object arg2, string argName2)
        {
            CheckIsNotNull(arg1, argName1);
            CheckIsNotNull(arg2, argName2);
        }

        public static void AreNotNull(object arg1, string argName1, object arg2, string argName2, object arg3, string argName3)
        {
            CheckIsNotNull(arg1, argName1);
            CheckIsNotNull(arg2, argName2);
            CheckIsNotNull(arg3, argName3);
        }

        public static void AreNotNull(object arg1, string argName1, object arg2, string argName2, object arg3, string argName3, object arg4, string argName4)
        {
            CheckIsNotNull(arg1, argName1);
            CheckIsNotNull(arg2, argName2);
            CheckIsNotNull(arg3, argName3);
            CheckIsNotNull(arg4, argName4);
        }

        private static void CheckIsNotNull(object arg, string argName)
        {
            if (arg == null) throw new ArgumentNullException(argName);
        }

        #endregion

        #region Not null or empty

        public static void IsNotNullOrEmpty(this string arg, string argName)
        {
            CheckIsNotNullOrEmpty(arg, argName);
        }

        public static void AreNotNullOrEmpty(string arg1, string argName1, string arg2, string argName2)
        {
            CheckIsNotNullOrEmpty(arg1, argName1);
            CheckIsNotNullOrEmpty(arg2, argName2);
        }

        public static void AreNotNullOrEmpty(string arg1, string argName1, string arg2, string argName2, string arg3, string argName3)
        {
            CheckIsNotNullOrEmpty(arg1, argName1);
            CheckIsNotNullOrEmpty(arg2, argName2);
            CheckIsNotNullOrEmpty(arg3, argName3);
        }

        public static void AreNotNullOrEmpty(string arg1, string argName1, string arg2, string argName2, string arg3, string argName3, string arg4, string argName4)
        {
            CheckIsNotNullOrEmpty(arg1, argName1);
            CheckIsNotNullOrEmpty(arg2, argName2);
            CheckIsNotNullOrEmpty(arg3, argName3);
            CheckIsNotNullOrEmpty(arg4, argName4);
        }

        private static void CheckIsNotNullOrEmpty(string arg, string argName)
        {
            if (arg.IsNullOrEmpty()) throw new ArgumentException("Argument '" + argName + "' is null or empty.", argName);
        } 

        #endregion

        #region Coll. not null or empty

        public static void IsNotNullOrEmpty<T>(this ICollection<T> arg, string argName)
        {
            CheckIsNotNullOrEmpty(arg, argName);
        }

        public static void AreNotNullOrEmpty<T>(ICollection<T> arg1, string argName1, ICollection<T> arg2, string argName2)
        {
            CheckIsNotNullOrEmpty(arg1, argName1);
            CheckIsNotNullOrEmpty(arg2, argName2);
        }

        public static void AreNotNullOrEmpty<T>(ICollection<T> arg1, string argName1, ICollection<T> arg2, string argName2, ICollection<T> arg3, string argName3)
        {
            CheckIsNotNullOrEmpty(arg1, argName1);
            CheckIsNotNullOrEmpty(arg2, argName2);
            CheckIsNotNullOrEmpty(arg3, argName3);
        }

        public static void ArgumentsAreNotNullOrEmpty<T>(ICollection<T> arg1, string argName1, ICollection<T> arg2, string argName2, ICollection<T> arg3, string argName3, ICollection<T> arg4, string argName4)
        {
            CheckIsNotNullOrEmpty(arg1, argName1);
            CheckIsNotNullOrEmpty(arg2, argName2);
            CheckIsNotNullOrEmpty(arg3, argName3);
            CheckIsNotNullOrEmpty(arg4, argName4);
        }

        private static void CheckIsNotNullOrEmpty<T>(ICollection<T> arg, string argName)
        {
            if (arg.IsNullOrEmpty()) throw new ArgumentException("Argument '" + argName + "' is null or empty.", argName);
        }

        #endregion

        #region Coll. not null or empty

        public static void IsNotNullOrEmpty<T>(this T[] arg, string argName)
        {
            CheckIsNotNullOrEmpty(arg, argName);
        }

        public static void AreNotNullOrEmpty<T>(T[] arg1, string argName1, T[] arg2, string argName2)
        {
            CheckIsNotNullOrEmpty(arg1, argName1);
            CheckIsNotNullOrEmpty(arg2, argName2);
        }

        public static void AreNotNullOrEmpty<T>(T[] arg1, string argName1, T[] arg2, string argName2, T[] arg3, string argName3)
        {
            CheckIsNotNullOrEmpty(arg1, argName1);
            CheckIsNotNullOrEmpty(arg2, argName2);
            CheckIsNotNullOrEmpty(arg3, argName3);
        }

        public static void AreNotNullOrEmpty<T>(T[] arg1, string argName1, T[] arg2, string argName2, T[] arg3, string argName3, T[] arg4, string argName4)
        {
            CheckIsNotNullOrEmpty(arg1, argName1);
            CheckIsNotNullOrEmpty(arg2, argName2);
            CheckIsNotNullOrEmpty(arg3, argName3);
            CheckIsNotNullOrEmpty(arg4, argName4);
        }

        private static void CheckIsNotNullOrEmpty<T>(T[] arg, string argName)
        {
            if (arg.IsNullOrEmpty()) throw new ArgumentException("Argument '" + argName + "' is null or empty.", argName);
        }

        #endregion

        #region Cast

        public static T CastAs<T>(this object arg, string argName) where T : class
        {
            Contract.Requires(arg != null);
            Contract.Requires(!argName.IsNullOrEmpty());
            Contract.Ensures(Contract.Result<T>() != null);

            return CastAs<T>(arg, argName, false);
        }

        public static T CastAs<T>(this object arg, string argName, bool allowsNull) where T : class
        {
            if (!allowsNull && object.ReferenceEquals(arg, null)) throw new ArgumentNullException(argName);
            var result = arg as T;
            if (result == null) throw new ArgumentException(string.Format("Argument '" + argName + "' is not a(n) '" + typeof(T) + "' object.", argName));
            return result;
        }

        public static T Cast<T>(this object arg, string argName)
        {
            Contract.Requires(arg != null);
            Contract.Requires(!argName.IsNullOrEmpty());

            return Cast<T>(arg, argName, false);
        }

        public static T Cast<T>(this object arg, string argName, bool allowsNull)
        {
            if (!allowsNull && object.ReferenceEquals(arg, null)) throw new ArgumentNullException(argName);
            if (!(arg is T)) throw new ArgumentException(string.Format("Argument '" + argName + "' is not a(n) '" + typeof(T) + "' object.", argName));
            return (T)arg;
        }

        #endregion
    }
}
