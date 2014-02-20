using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using Neuroflow.Design.ActivityDesigners;

namespace Neuroflow.Activities
{
    [Designer(typeof(ExperimentBranchDesigner))]
    public sealed class ExperimentBranch : NativeActivity
    {
        public bool IsActive { get; set; }

        [Browsable(false)]
        public Activity Body { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddChild(Body);
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (IsActive && Body != null) context.ScheduleActivity(Body);
        }
    }
}
