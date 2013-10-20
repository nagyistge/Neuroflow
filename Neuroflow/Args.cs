using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    public static class Args
    {
        readonly static Registry<Tuple<string, string, int, string>, Delegate> compiledExpressions = new Registry<Tuple<string, string, int, string>, Delegate>();

        [Conditional("DEBUG")]
        public static void Requires<T>(Expression<Func<T>> member, Func<bool> expr, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            string argName = null;
            Func<object> getArgValue = null;

            if (member != null)
            {
                var memEx = member.Body as MemberExpression;
                if (memEx != null)
                {
                    argName = memEx.Member.Name;
                    getArgValue = () =>
                    {
                        var key = Tuple.Create(memberName, sourceFilePath, sourceLineNumber, argName);
                        var lambda = (Func<T>)compiledExpressions.GetOrCreate(key, () => member.Compile());
                        return lambda();
                    };
                }
            }

            CheckRequires(argName, getArgValue, expr, memberName, sourceFilePath, sourceLineNumber);
        }

        private static void CheckRequires(string argName, Func<object> getArgValue, Func<bool> expr, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            if (!expr())
            {
                var argValue = getArgValue != null ? getArgValue() : null;

                if (getArgValue != null && argValue == null && argName != null)
                {
                    throw new ArgumentNullException(
                        argName,
                        string.Format("Invalid argument value{3}{4}. Member name: '{0}', source file: '{1}', line number: {2}.",
                        memberName,
                        sourceFilePath,
                        sourceLineNumber,
                        argValue == null ? " null" : string.Format(" '{0}'", argValue),
                        string.IsNullOrWhiteSpace(argName) ? string.Empty : string.Format(" of argument '{0}'", argName)));
                }
                else
                {
                    throw new ArgumentException(
                        string.Format("Invalid argument value{3}{4}. Member name: '{0}', source file: '{1}', line number: {2}.",
                        memberName,
                        sourceFilePath,
                        sourceLineNumber,
                        argValue == null ? " <null>" : argValue as string == string.Empty ? " string.Empty" : string.Format(" '{0}'", argValue),
                        string.IsNullOrWhiteSpace(argName) ? string.Empty : string.Format(" of argument '{0}'", argName)));
                }
            }
        }
    }
}
