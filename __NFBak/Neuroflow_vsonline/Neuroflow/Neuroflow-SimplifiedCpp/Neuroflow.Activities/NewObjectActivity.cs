using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Activities.Statements;
using System.Activities.Expressions;
using System.ComponentModel;
using System.Reflection;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Neuroflow.Core;
using Neuroflow.Core.ComponentModel;
using Microsoft.CSharp.Activities;

namespace Neuroflow.Activities
{
    #region PropertyValue Classes

    [Serializable]
    public abstract class PropertyValue
    {
        public string Name { get; set; }
        public Type PropertyType { get; set; }
        public bool ForProperty { get; set; }
        internal Attribute[] Attributes { get; set; }
    }

    [Serializable]
    public sealed class InArgumentValue : PropertyValue
    {
        [NonSerialized]
        InArgument inArgument;

        public InArgument InArgument
        {
            get { return inArgument; }
            set { inArgument = value; }
        }
    }

    [Serializable]
    public sealed class ActivityWithResultValue : PropertyValue
    {
        [NonSerialized]
        ActivityWithResult activity;

        public ActivityWithResult Activity
        {
            get { return activity; }
            set { activity = value; }
        }

        public Type ActivityResultType { get; set; }

        bool? isCollection;

        public bool IsCollection
        {
            get
            {
                if (isCollection == null)
                {
                    isCollection = ActivityResultType.GetGenericTypeDefinition() == typeof(Collection<>);
                }
                return isCollection.Value;
            }
        }
    }

    #endregion

    [Designer(Designers.NewObjectActivityDesigner)]
    public abstract class NewObjectActivity<T> : NativeActivity<T>, ICustomTypeDescriptor, IEnsureInitialization
        where T : class
    {
        #region Types

        class ContainedValuePropertyDescriptor : PropertyDescriptor
        {
            public ContainedValuePropertyDescriptor(Type ownerType, InArgumentValue inArgValue)
                : base(inArgValue.Name, inArgValue.Attributes)
            {
                this.ownerType = ownerType;
                this.propType = inArgValue.PropertyType;
                this.getValue = (component) => ((NewObjectActivity<T>)component).InArgumentProperties[inArgValue.Name].InArgument;
                this.setValue = (component, value) => ((NewObjectActivity<T>)component).InArgumentProperties[inArgValue.Name].InArgument = value as InArgument;
            }

            public ContainedValuePropertyDescriptor(Type ownerType, ActivityWithResultValue activityValue)
                : base(activityValue.Name, activityValue.Attributes)
            {
                this.ownerType = ownerType;
                this.propType = activityValue.PropertyType;
                this.getValue = (component) => ((NewObjectActivity<T>)component).ActivityWithResultProperties[activityValue.Name].Activity;
                this.setValue = (component, value) => ((NewObjectActivity<T>)component).ActivityWithResultProperties[activityValue.Name].Activity = value as ActivityWithResult;
            }

            Type ownerType;

            Type propType;

            Func<object, object> getValue;

            Action<object, object> setValue;

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get { return ownerType; }
            }

            public override object GetValue(object component)
            {
                return getValue(component);
            }

            public override bool IsReadOnly
            {
                get { return setValue == null; }
            }

            public override Type PropertyType
            {
                get { return propType; }
            }

            public override void ResetValue(object component)
            {
            }

            public override void SetValue(object component, object value)
            {
                if (setValue != null) setValue(component, value);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return true;
            }
        }

        #endregion

        #region Construct

        public NewObjectActivity()
        {
            CreatedValues = new Variable<Dictionary<string, object>>();
            DisplayName = ObjectType.Name;
        }

        #endregion

        #region Properties and Fields

        protected Variable<Dictionary<string, object>> CreatedValues { get; private set; }

        Variable<Queue<string>> remainingActivityProps = new Variable<Queue<string>>();

        protected virtual Type ObjectType
        {
            get { return typeof(T); }
        }

        Dictionary<string, InArgumentValue> inArgumentProperties;

        [Browsable(false)]
        public Dictionary<string, InArgumentValue> InArgumentProperties
        {
            get { return inArgumentProperties ?? (inArgumentProperties = new Dictionary<string, InArgumentValue>()); }
        }

        Dictionary<string, ActivityWithResultValue> activityWithResultProperties;

        [Browsable(false)]
        public Dictionary<string, ActivityWithResultValue> ActivityWithResultProperties
        {
            get { return activityWithResultProperties ?? (activityWithResultProperties = new Dictionary<string, ActivityWithResultValue>()); }
        }

        [Browsable(false)]
        public bool IsInitialized { get; set; } 

        #endregion

        #region Initialize

        public void EnsureInitialization()
        {
            if (!IsInitialized)
            {
                foreach (var reflectedItem in NewObjectActivityHelpers.GetReflectedItems(ObjectType))
                {
                    var attributes = reflectedItem.Attributes;

                    if (attributes.OfType<BrowsableAttribute>().Any(a => !a.Browsable)) continue;

                    if (reflectedItem.Type.IsValueType || reflectedItem.Type == typeof(string))
                    {
                        if (reflectedItem.CanRead && reflectedItem.CanWrite)
                        {
                            // Standard value property:

                            AddInArgProperty(reflectedItem, attributes);
                        }
                    }
                    else
                    {
                        var collItemType = NewObjectActivityHelpers.GetCollectionItemType(reflectedItem.Type);
                        if (collItemType != null && (collItemType.IsValueType || collItemType == typeof(string)))
                        {
                            AddInArgProperty(reflectedItem, attributes);
                        }
                        else if ((collItemType == null) && (reflectedItem.CanRead && reflectedItem.CanWrite) ||
                                 (collItemType != null) && (reflectedItem.CanRead))

                        {
                            var propType = collItemType == null ?
                                NewObjectActivityHelpers.GetActivityWithResultType(reflectedItem.Type) :
                                NewObjectActivityHelpers.GetActivityWithResultCollectionType(collItemType);

                            var resultType = collItemType == null ?
                                reflectedItem.Type :
                                NewObjectActivityHelpers.GetCollectionType(collItemType);

                            ActivityWithResultProperties.Add(
                                    reflectedItem.Name,
                                    new ActivityWithResultValue
                                    {
                                        PropertyType = propType,
                                        ActivityResultType = resultType,
                                        Attributes = attributes,
                                        Name = reflectedItem.Name,
                                        ForProperty = reflectedItem.IsProperty
                                    });
                        }
                    }
                }
                IsInitialized = true;
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            EnsureInitialization();

            CacheDynamicMetadata(metadata);

            metadata.AddImplementationVariable(CreatedValues);
            metadata.AddImplementationVariable(remainingActivityProps);
        }

        private void CacheDynamicMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument arg;
            foreach (var inArgProp in InArgumentProperties.Values)
            {
                inArgProp.Attributes = NewObjectActivityHelpers.GetAttributes(NewObjectActivityHelpers.GetAttributeProvider(ObjectType, inArgProp));
                var inArg = inArgProp.InArgument;
                arg = NewObjectActivityHelpers.CreateRuntimeArgument(inArgProp.Name, inArgProp.InArgument.ArgumentType, inArgProp.Attributes);
                metadata.Bind(inArg, arg);
                metadata.AddArgument(arg);
            }

            foreach (var awrProp in ActivityWithResultProperties.Values)
            {
                awrProp.Attributes = NewObjectActivityHelpers.GetAttributes(NewObjectActivityHelpers.GetAttributeProvider(ObjectType, awrProp));

                NewObjectActivityHelpers.AddChildActivityWithResult(metadata, awrProp.Activity, awrProp.ActivityResultType, awrProp.Name, awrProp.Attributes);
            }
        }

        private void AddInArgProperty(ReflectedItem item, Attribute[] attributes)
        {
            var initValueAttrib = attributes.OfType<InitValueAttribute>().FirstOrDefault();

            InArgument inArg;
            if (initValueAttrib != null && initValueAttrib.Value != null)
            {
                var method = NewObjectActivityHelpers.GetGenericMethod(this, initValueAttrib.Value.GetType(), "CreateTypeInArgument");
                inArg = (InArgument)method.Invoke(this, new[] { initValueAttrib.Value });
            }
            else
            {
                inArg = (InArgument)Argument.Create(item.Type, ArgumentDirection.In);
            }

            InArgumentProperties.Add(
                item.Name,
                new InArgumentValue
                {
                    Name = item.Name,
                    InArgument = inArg,
                    PropertyType = inArg.GetType(),
                    Attributes = attributes,
                    ForProperty = item.IsProperty
                });
        }

        public InArgument<TValue> CreateTypeInArgument<TValue>(TValue defaultValue)
        {
            //if (typeof(TValue).IsEnum)
            //{
            //    return new InArgument<TValue>(new CSharpValue<TValue>(typeof(TValue).Name + '.' + defaultValue.ToString()));
            //}
            //else
            {
                return new InArgument<TValue>(defaultValue);
            }
        }

        #endregion

        #region Execute

        protected override void Execute(NativeActivityContext context)
        {
            StartCreatingValues(context);
        }

        private void StartCreatingValues(NativeActivityContext context)
        {
            var createdValueStore = new Dictionary<string, object>();
            CreatedValues.Set(context, createdValueStore);

            foreach (var inArgProp in InArgumentProperties.Values)
            {
                createdValueStore.Add(inArgProp.Name, inArgProp.InArgument.Get(context));
            }

            var remActProps = new Queue<string>();
            remainingActivityProps.Set(context, remActProps);
            foreach (var activityProp in this.ActivityWithResultProperties.Values)
            {
                if (activityProp.Activity != null) remActProps.Enqueue(activityProp.Name);
            }

            ContinueCreatingValues(context);
        }

        private void ContinueCreatingValues(NativeActivityContext context)
        {
            var remActProps = remainingActivityProps.Get(context);
            if (remActProps.Count != 0)
            {
                string nextProp = remActProps.Peek();
                var activityProp = ActivityWithResultProperties[nextProp];
                if (activityProp.Activity != null)
                {
                    CallScheduleActivityProp(context, activityProp);
                    return;
                }
                else
                {
                    remActProps.Dequeue();
                    ContinueCreatingValues(context);
                    return;
                }
            }

            ValuesCreated(context);
        }

        #region ScheduleActivityProp

        private void CallScheduleActivityProp(NativeActivityContext context, ActivityWithResultValue value)
        {
            var mi = NewObjectActivityHelpers.GetGenericMethod(this, value.ActivityResultType, "ScheduleActivityProp");
            mi.Invoke(this, new object[] { context, value.Activity });
        }

        public void ScheduleActivityProp<TResult>(NativeActivityContext context, Activity<TResult> activity)
        {
            context.ScheduleActivity(activity, OnActivityPropCompleted);
        }

        void OnActivityPropCompleted<TResult>(NativeActivityContext context, ActivityInstance completedInstance, TResult result)
        {
            var remActProps = remainingActivityProps.Get(context);
            string currentProp = remActProps.Dequeue();
            var createdValueStore = CreatedValues.Get(context);

            createdValueStore.Add(currentProp, result);

            ContinueCreatingValues(context);
        } 

        #endregion

        #region Values Created

        protected virtual void ValuesCreated(NativeActivityContext context)
        {
            var values = CreatedValues.Get(context);
            var propValues = GetPropertyValues();
            var result = NewObjectActivityHelpers.CreateObject<T>(ObjectType, propValues, values);
            Result.Set(context, result);
        }

        protected IEnumerable<PropertyValue> GetPropertyValues()
        {
            var propValues = InArgumentProperties.Values.Cast<PropertyValue>()
                .Concat(ActivityWithResultProperties.Values.Cast<PropertyValue>())
                .Where(v => v.ForProperty);
            return propValues;
        }

        #endregion

        #endregion

        #region ICustomTypeDescriptor

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            var reflectionDescriptors = 
                attributes != null ? 
                    TypeDescriptor.GetProperties(this, attributes, true) : 
                    TypeDescriptor.GetProperties(this, true);

            var result = new List<PropertyDescriptor>(reflectionDescriptors.Cast<PropertyDescriptor>());

            foreach (var inArgValue in this.InArgumentProperties.Values)
            {
                result.Add(new ContainedValuePropertyDescriptor(GetType(), inArgValue));
            }

            foreach (var activityValue in this.ActivityWithResultProperties.Values)
            {
                result.Add(new ContainedValuePropertyDescriptor(GetType(), activityValue));
            }

            return new PropertyDescriptorCollection(result.ToArray(), true);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(null);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
    }
}
