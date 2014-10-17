using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Diagnostics.Contracts;
using System.Activities.Presentation.Model;

namespace NeoComp.Activities.Internal
{
    internal static class Helpers
    {
        internal static Activity Find(this Activity root, string displayNamePath)
        {
            Contract.Requires(root != null);
            Contract.Requires(!string.IsNullOrEmpty(displayNamePath));

            string[] path = displayNamePath.Split(new[] { '.' });
            var current = root;
            int pIdx = 0;
            do
            {
                string nextName = path[pIdx++];
                current = WorkflowInspectionServices.GetActivities(current).Where(a => a.DisplayName == nextName).FirstOrDefault();
                if (current == null) return null;
            }
            while (pIdx < path.Length);
            return current;
        }

        internal static bool IsNull(this ActivityDelegate activityDelegate)
        {
            return activityDelegate == null || activityDelegate.Handler == null;
        }

        internal static string CreateCacheKey(this Activity activity, string key)
        {
            Contract.Requires(activity != null);
            Contract.Requires(!String.IsNullOrEmpty(key));

            return activity.Id + key;
        }
    }
}
