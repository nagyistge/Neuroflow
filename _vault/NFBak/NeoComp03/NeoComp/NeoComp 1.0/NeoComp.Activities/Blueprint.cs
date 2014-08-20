using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Reflection;
using NeoComp.Internal;
using System.Diagnostics.Contracts;
using System.Activities.Presentation;
using Microsoft.VisualBasic.Activities;
using System.ComponentModel;
using NeoComp.Activities.Design;

namespace NeoComp.Activities
{
    static class Blueprint
    {
        static Dictionary<PropertyInfo, BlueprintFuncAttribute> bpFuncAttribs = new Dictionary<PropertyInfo, BlueprintFuncAttribute>();

        static Dictionary<PropertyInfo, DefaultExpressionAttribute> defExprAttribs = new Dictionary<PropertyInfo, DefaultExpressionAttribute>();
        
        static Dictionary<Type, IPropertyOrFieldAccessor[]> FuncProperties = new Dictionary<Type, IPropertyOrFieldAccessor[]>();

        static Dictionary<Type, IPropertyOrFieldAccessor[]> InArgProperties = new Dictionary<Type, IPropertyOrFieldAccessor[]>();

        internal static BlueprintFuncAttribute GetBlueprintFuncAttribute(PropertyInfo propInfo)
        {
            Contract.Requires(propInfo != null);
            
            return bpFuncAttribs.GetOrRegister(propInfo, () => propInfo.GetCustomAttributes(typeof(BlueprintFuncAttribute), false).Cast<BlueprintFuncAttribute>().FirstOrDefault());
        }

        internal static DefaultExpressionAttribute GetDefaultExpressionAttribute(PropertyInfo propInfo)
        {
            Contract.Requires(propInfo != null);

            return defExprAttribs.GetOrRegister(propInfo, () => propInfo.GetCustomAttributes(typeof(DefaultExpressionAttribute), true).Cast<DefaultExpressionAttribute>().FirstOrDefault());
        }

        internal static IPropertyOrFieldAccessor[] GetFuncProperties<T>(this Blueprint<T> blueprint)
        {
            Contract.Requires(blueprint != null);

            var q = from prop in blueprint.GetType().GetProperties()
                    let oa = GetBlueprintFuncAttribute(prop)
                    where oa != null && prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(ActivityFunc<>)
                    orderby oa.Order
                    select AccessorFactory.CreatePropertyOrFieldAccessor(prop);

            return FuncProperties.GetOrRegister(blueprint.GetType(), () => q.ToArray());
        }

        internal static IPropertyOrFieldAccessor[] GetInArgProperties<T>(this Blueprint<T> blueprint)
        {
            Contract.Requires(blueprint != null);

            var q = from prop in blueprint.GetType().GetProperties()
                    where prop.PropertyType.IsSubclassOf(typeof(InArgument))
                    select AccessorFactory.CreatePropertyOrFieldAccessor(prop);

            return InArgProperties.GetOrRegister(blueprint.GetType(), () => q.ToArray());
        }
    }

    [Designer(typeof(BlueprintDesigner))]
    public abstract class Blueprint<T> : NativeActivity<T>, IActivityTemplateFactory
    {
        #region Properties

        Variable<int> currentFuncIndex;

        public Variable<int> CurrentFuncIndex
        {
            get { return currentFuncIndex ?? (currentFuncIndex = new Variable<int>()); }
        }

        Variable<string> currentFuncPropName;

        public Variable<string> CurrentFuncPropName
        {
            get { return currentFuncPropName ?? (currentFuncPropName = new Variable<string>()); }
        }

        Dictionary<string, Variable<object>> resultVariables;

        private Dictionary<string, Variable<object>> ResultVariables
        {
            get 
            {
                if (resultVariables == null)
                {
                    resultVariables = new Dictionary<string, Variable<object>>();
                    AddResultVariables();
                }
                return resultVariables;
            }
        } 

        #endregion

        #region Metadata

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            // Args:
            RuntimeArgument arg;
            foreach (var accessor in Blueprint.GetInArgProperties(this))
            {
                metadata.AddArgument(arg = new RuntimeArgument(accessor.Name, accessor.PropertyOrFieldType.GetGenericArguments()[0], ArgumentDirection.In));
                metadata.Bind((InArgument)accessor.FastGet(this), arg);
            }

            // Funcs:
            foreach (var accessor in Blueprint.GetFuncProperties(this))
            {
                metadata.AddDelegate((ActivityDelegate)accessor.FastGet(this));
            }

            // Vars:
            foreach (var variable in ResultVariables.Values)
            {
                metadata.AddImplementationVariable(variable);
            }

            metadata.AddImplementationVariable(CurrentFuncPropName);
            metadata.AddImplementationVariable(CurrentFuncIndex);
        }

        #endregion

        #region Impl

        protected override void Execute(NativeActivityContext context)
        {
            CurrentFuncIndex.Set(context, 0);
            CurrentFuncPropName.Set(context, null);
            NextStep(context);
        }

        private void NextStep(NativeActivityContext context)
        {
            var funcProps = Blueprint.GetFuncProperties(this);

            int currentFuncIndex = CurrentFuncIndex.Get(context);

            if (currentFuncIndex == funcProps.Length)
            {
                Done(context); // We Are Done.
            }
            else
            {
                var funcAccessor = funcProps[currentFuncIndex++];
                CurrentFuncIndex.Set(context, currentFuncIndex);
                CurrentFuncPropName.Set(context, funcAccessor.Name);
                var func = (ActivityDelegate)funcAccessor.FastGet(this);
                if (func == null)
                {
                    NextStep(context); // Func is null, go for next.
                }
                else
                {
                    context.ScheduleDelegate(func, null, FuncDoneCallback);
                }
            }
        }

        void FuncDoneCallback(
            NativeActivityContext context, 
            ActivityInstance completedInstance, 
            IDictionary<string, Object> outArguments)
        {
            var propName = CurrentFuncPropName.Get(context);
            if (outArguments != null)
            {
                object result;
                if (outArguments.TryGetValue("Result", out result))
                {
                    ResultVariables[propName].Set(context, result);
                }
            }
            NextStep(context);
        }

        private void Done(NativeActivityContext context)
        {
            var obj = CreateObject(context);
            Result.Set(context, obj);
        }

        protected abstract T CreateObject(NativeActivityContext context);

        protected RT GetFuncResult<RT>(NativeActivityContext context, string name)
            where RT : class
        {
            Variable<object> resultVar;
            if (ResultVariables.TryGetValue(name, out resultVar))
            {
                return resultVar.Get(context) as RT;
            }
            return null;
        }

        #endregion

        #region Add Result Variables

        private void AddResultVariables()
        {
            foreach (var accessor in Blueprint.GetFuncProperties(this))
            {
                ResultVariables[accessor.Name] = new Variable<object>(accessor.Name);
            }
        } 

        #endregion

        #region IActivityTemplateFactory Members

        Activity IActivityTemplateFactory.Create(System.Windows.DependencyObject target)
        {
            var activity = CreateActivityTemplate(target);
            var tmpl = activity as Blueprint<T>;

            if (tmpl == null) return activity;
            
            foreach (var accessor in Blueprint.GetFuncProperties(tmpl))
            {
                var func = (ActivityDelegate)accessor.FastGet(tmpl);
                if (func == null)
                {
                    var bpfa = Blueprint.GetBlueprintFuncAttribute(accessor.PropertyInfo);
                    var varName = bpfa.ResultVariableName;
                    if (string.IsNullOrWhiteSpace(varName))
                    {
                        varName = accessor.Name;
                        varName = char.ToLower(varName[0]) + varName.Substring(1) + "Result";
                    }
                    var garg = accessor.PropertyOrFieldType.GetGenericArguments()[0];
                    var outArgType = typeof(DelegateOutArgument<>).MakeGenericType(garg);
                    var outArg = (DelegateOutArgument)Activator.CreateInstance(outArgType, varName);
                    func = (ActivityDelegate)Activator.CreateInstance(accessor.PropertyOrFieldType);
                    ((dynamic)func).Result = (dynamic)outArg;
                    accessor.FastSet(tmpl, func);
                }
            }

            foreach (var accessor in Blueprint.GetInArgProperties(tmpl))
            {
                var inArg = (InArgument)accessor.FastGet(tmpl);
                if (inArg == null)
                {
                    var defExA = Blueprint.GetDefaultExpressionAttribute(accessor.PropertyInfo);
                    if (defExA != null && !string.IsNullOrWhiteSpace(defExA.Expression))
                    {
                        var garg = accessor.PropertyOrFieldType.GetGenericArguments()[0];
                        var vbVType = typeof(VisualBasicValue<>).MakeGenericType(garg);
                        var vbVArg = Activator.CreateInstance(vbVType, defExA.Expression);
                        inArg = (InArgument)Activator.CreateInstance(accessor.PropertyOrFieldType, vbVArg);
                        accessor.FastSet(tmpl, inArg);
                    }
                    else
                    {
                        inArg = (InArgument)Activator.CreateInstance(accessor.PropertyOrFieldType);
                        accessor.FastSet(tmpl, inArg);
                    }
                }
            }

            return tmpl;
        }

        protected abstract Activity CreateActivityTemplate(System.Windows.DependencyObject target);

        #endregion
    }
}
