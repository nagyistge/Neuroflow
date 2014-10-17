using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace NeoComp.Activities.Design.Behaviors
{
    public sealed class RememberIsExpanded : RememberValues
    {
        static RememberIsExpanded()
        {
            RememberValues.EventsProperty.OverrideMetadata(
                typeof(RememberIsExpanded),
                new UIPropertyMetadata(new EventCollection { Expander.CollapsedEvent, Expander.ExpandedEvent }));

            RememberValues.PropertiesProperty.OverrideMetadata(
                typeof(RememberIsExpanded),
                new UIPropertyMetadata(new PropertyCollection { Expander.IsExpandedProperty }));
        }
    }
}
