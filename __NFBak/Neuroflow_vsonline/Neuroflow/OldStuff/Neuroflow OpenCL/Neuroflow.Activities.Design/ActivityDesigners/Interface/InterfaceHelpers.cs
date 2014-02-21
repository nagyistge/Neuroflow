using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.Model;
using System.Diagnostics.Contracts;
using System.Activities;
using Neuroflow.Core;

namespace Neuroflow.Activities.Design.ActivityDesigners.Interface
{
    internal static class InterfaceHelpers
    {
        internal static void EnsureInitialization(ModelItem modelItem)
        {
            Contract.Requires(modelItem != null);

            var value = modelItem.GetCurrentValue();
            var ei = value as IEnsureInitialization;
            if (ei != null) ei.EnsureInitialization();
        }
    }
}
