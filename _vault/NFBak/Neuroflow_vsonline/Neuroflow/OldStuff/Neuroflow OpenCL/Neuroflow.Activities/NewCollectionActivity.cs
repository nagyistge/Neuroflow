using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Collections.ObjectModel;
using System.Activities.Statements;
using System.ComponentModel;

namespace Neuroflow.Activities
{
    internal static class NewCollectionActivityHelpers
    {
        internal static ActivityWithResult Create(Type itemType)
        {
            var mi = typeof(NewCollectionActivityHelpers).GetMethod("CreateGeneric");
            var gmi = mi.MakeGenericMethod(itemType);
            return (ActivityWithResult)gmi.Invoke(null, null);
        }

        public static NewCollectionActivity<T> CreateGeneric<T>()
        {
            return new NewCollectionActivity<T>();
        }
    }
    
    [Designer(Designers.NewCollectionDesigner)]
    public class NewCollectionActivity<T> : NativeActivity<Collection<T>>
    {
        public NewCollectionActivity()
        {
            CreateItemActivities = new Collection<Activity<T>>();
            DisplayName = typeof(T).Name + "Collection";
            if (typeof(T).IsInterface && DisplayName[0] == 'I')
            {
                DisplayName = DisplayName.Substring(1);
            }
        }

        public Type AllowedActivityType
        {
            get { return typeof(Activity<T>); }
        }

        public Collection<Activity<T>> CreateItemActivities { get; private set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            foreach (var cia in CreateItemActivities) metadata.AddChild(cia);
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (CreateItemActivities.Count != 0)
            {
                foreach (var cia in CreateItemActivities)
                {
                    context.ScheduleActivity(cia, OnCreateItemCompleted);
                }
            }
            else
            {
                Result.Set(context, new Collection<T>());
            }
        }

        private void OnCreateItemCompleted(NativeActivityContext context, ActivityInstance instance, T item)
        {
            var resultColl = Result.Get(context);
            if (resultColl == null)
            {
                resultColl = new Collection<T>();
                Result.Set(context, resultColl);
            }
            resultColl.Insert(0, item);
        }
    }
}
