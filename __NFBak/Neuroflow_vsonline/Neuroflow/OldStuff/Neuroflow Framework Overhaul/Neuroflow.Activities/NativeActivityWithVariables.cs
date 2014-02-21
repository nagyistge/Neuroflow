using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Activities;

namespace Neuroflow.Activities
{
    public abstract class NativeActivityWithVariables : NativeActivity
    {
        Collection<Variable> variables;

        public Collection<Variable> Variables
        {
            get { return variables ?? (variables = new Collection<Variable>()); }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (variables != null)
            {
                foreach (var v in Variables)
                {
                    metadata.AddVariable(v);
                }
            }
        }
    }

    public abstract class NativeActivityWithVariables<T> : NativeActivity<T>
    {
        Collection<Variable> variables;

        public Collection<Variable> Variables
        {
            get { return variables ?? (variables = new Collection<Variable>()); }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (variables != null)
            {
                foreach (var v in Variables)
                {
                    metadata.AddVariable(v);
                }
            }
        }
    }
}
