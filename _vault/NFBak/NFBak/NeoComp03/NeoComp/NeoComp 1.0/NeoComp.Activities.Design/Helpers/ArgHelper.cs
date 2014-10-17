using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.Model;
using System.Activities;
using Microsoft.VisualBasic.Activities;
using System.Diagnostics.Contracts;

namespace NeoComp.Activities.Design.Helpers
{
    public interface IArgHelper
    {
        void Set(ModelProperty argProp, string expression);

        string Get(ModelProperty argProp);
    }

    public static class ArgHelper
    {
        public static IArgHelper Create(ModelProperty property)
        {
            Contract.Requires(property != null);

            var type = property.PropertyType;
            var ga = type.GetGenericArguments()[0];
            var helperType = typeof(ArgHelper<>).MakeGenericType(ga);
            var helper = (IArgHelper)Activator.CreateInstance(helperType);
            return helper;
        }
    }
    
    internal sealed class ArgHelper<T> : IArgHelper
    {
        public void Set(ModelProperty argProp, string expression)
        {
            string current = Get(argProp);
            if (current != expression)
            {
                if (argProp.PropertyType.IsSubclassOf(typeof(OutArgument)))
                {
                    argProp.SetValue(new OutArgument<T>(new VisualBasicReference<T>(expression)));
                }
                else
                {
                    argProp.SetValue(new InArgument<T>(new VisualBasicValue<T>(expression)));
                }
            }
        }

        public string Get(ModelProperty argProp)
        {
            if (argProp.Value == null) return null;

            var value = argProp.Value.GetCurrentValue();
            if (value == null) return null;

            var outArgValue = value as OutArgument<T>;
            if (outArgValue != null)
            {
                var outArgRef = outArgValue.Expression as VisualBasicReference<T>;
                if (outArgRef != null) return outArgRef.ExpressionText;
                return null;
            }

            var inArgValue = value as InArgument<T>;
            if (inArgValue != null)
            {
                var inArgRef = inArgValue.Expression as VisualBasicValue<T>;
                if (inArgRef != null) return inArgRef.ExpressionText;
                return null;
            }

            return null;
        }
    }
}
