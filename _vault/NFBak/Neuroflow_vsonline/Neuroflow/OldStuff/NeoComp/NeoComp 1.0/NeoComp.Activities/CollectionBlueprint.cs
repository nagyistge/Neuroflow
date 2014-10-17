using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Activities.Design;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Activities;
using System.Windows.Markup;
using System.Activities.Presentation;
using NeoComp.Activities.Design.Helpers;

namespace NeoComp.Activities
{
    [CollectionBlueprintActivityMark]
    [Designer(typeof(CollectionBlueprintDesigner))]
    public abstract class CollectionBlueprint<T> : NativeActivity<T[]>, IActivityTemplateFactory
    {
        public CollectionBlueprint()
        {
            string typeName = typeof(T).Name;
            ItemResultName = typeName.CamelCase() + "ItemResult";
        }

        [Browsable(false)]
        public string ItemResultName { get; set; }

        Variable<List<T>> items;

        [Browsable(false)]
        public Variable<List<T>> Items
        {
            get { return items ?? (items = new Variable<List<T>>()); }
        }

        Variable<int> count;

        [Browsable(false)]
        [DependsOn("Items")]
        public Variable<int> Count
        {
            get { return count ?? (count = new Variable<int>()); }
        }

        Variable<int> toCount;

        [Browsable(false)]
        [DependsOn("Items")]
        public Variable<int> ToCount
        {
            get { return toCount ?? (toCount = new Variable<int>()); }
        }

        Collection<Activity> activities;

        [Browsable(false)]
        [DependsOn("Items")]
        [DependsOn("Count")]
        [DependsOn("ToCount")]
        public Collection<Activity> Activities
        {
            get { return activities ?? (activities = new Collection<Activity>()); }
        }

        List<ActivityFunc<T>> activityFuncs;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddImplementationVariable(Items);
            metadata.AddImplementationVariable(Count);
            metadata.AddImplementationVariable(ToCount);

            activityFuncs = Activities.Select(a =>
                new ActivityFunc<T>
                {
                    Handler = a,
                    Result = new DelegateOutArgument<T>(ItemResultName)
                }).ToList();

            foreach (var af in activityFuncs) metadata.AddDelegate(af);
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (activityFuncs != null && activityFuncs.Count > 0)
            {
                int toCount = 0;
                foreach (var af in activityFuncs)
                {
                    context.ScheduleFunc(af, OnCreateItemCompleted);
                    toCount++;
                }
                ToCount.Set(context, toCount);
            }
        }

        private void OnCreateItemCompleted(NativeActivityContext context, ActivityInstance instance, T item)
        {
            if (item != null)
            {
                var coll = Items.Get(context);
                if (coll == null)
                {
                    coll = new List<T>(Activities.Count);
                    Items.Set(context, coll);
                }
                coll.Add(item);
            }
            int count = Count.Get(context);
            count++;
            int toCount = ToCount.Get(context);

            if (count == toCount)
            {
                var coll = Items.Get(context);
                if (coll == null)
                {
                    Result.Set(context, new T[0]);
                }
                else
                {
                    Result.Set(context, coll.Where(i => i != null).ToArray());
                }                
            }
            else
            {
                Count.Set(context, count);
            }
        }

        #region IActivityTemplateFactory Members

        Activity IActivityTemplateFactory.Create(System.Windows.DependencyObject target)
        {
            return CreateCollectionBlueprintTemplate(target);
        }

        protected abstract Activity CreateCollectionBlueprintTemplate(System.Windows.DependencyObject target);

        #endregion
    }
}
