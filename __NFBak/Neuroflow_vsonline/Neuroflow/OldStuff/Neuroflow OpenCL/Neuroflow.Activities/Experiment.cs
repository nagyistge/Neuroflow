using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Activities.Presentation;
using System.ComponentModel;

namespace Neuroflow.Activities
{
    [Designer(Designers.ExperimentDesigner)]
    public sealed class Experiment : NativeActivityWithVariables, IActivityTemplateFactory
    {
        public Type BranchType { get { return typeof(ExperimentBranch); } }
        
        Collection<ExperimentBranch> branches;

        public Collection<ExperimentBranch> Branches
        {
            get { return branches ?? (branches = new Collection<ExperimentBranch>()); }
        }

        protected override void CacheMetadata(System.Activities.NativeActivityMetadata metadata)
        {
            if (branches != null && branches.Any(b => b.IsActive))
            {
                foreach (var b in branches) metadata.AddChild(b);
            }
            else
            {
                metadata.AddValidationError("At least one active Experiment Branch is required.");
            }
        }

        protected override void Execute(System.Activities.NativeActivityContext context)
        {
            var ab = Branches.FirstOrDefault(b => b.IsActive);
            if (ab != null) context.ScheduleActivity(ab);
        }

        System.Activities.Activity IActivityTemplateFactory.Create(System.Windows.DependencyObject target)
        {
            return new Experiment
            {
                Branches =
                {
                    new ExperimentBranch
                    {
                        IsActive = true
                    },
                    new ExperimentBranch()
                }
            };
        }
    }
}
