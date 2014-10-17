using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows;
using NeoComp.Activities.Design.Helpers;

namespace NeoComp.Activities.Design.Behaviors
{
    public class RememberValues : Behavior<UIElement>
    {
        public string StoreName
        {
            get { return (string)GetValue(StoreNameProperty); }
            set { SetValue(StoreNameProperty, value); }
        }

        public static readonly DependencyProperty StoreNameProperty =
            DependencyProperty.Register("StoreName", typeof(string), typeof(RememberValues), new UIPropertyMetadata(null));

        public EventCollection Events
        {
            get { return (EventCollection)GetValue(EventsProperty); }
            set { SetValue(EventsProperty, value); }
        }

        public static readonly DependencyProperty EventsProperty =
            DependencyProperty.Register("Events", typeof(EventCollection), typeof(RememberValues), new UIPropertyMetadata(null));
        
        public PropertyCollection Properties
        {
            get { return (PropertyCollection)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }

        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register("Properties", typeof(PropertyCollection), typeof(RememberValues), new UIPropertyMetadata(null));

        ViewStateManager vsm;

        private string GetKey(DependencyProperty prop)
        {
            string storeName = StoreName;
            if (storeName == null) storeName = AssociatedObject.GetType().Name + "." + prop.Name;
            return storeName + ":" + prop.Name;
        }

        protected override void OnAttached()
        {
            var props = Properties;
            var events = Events;
            if (props != null && props.Count != 0 && events != null && events.Count != 0)
            {
                vsm = new ViewStateManager(AssociatedObject);
                Dispatcher.BeginInvoke(new Action(RestoreValues));
                Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        foreach (var ev in Events)
                        {
                            AssociatedObject.AddHandler(ev, new RoutedEventHandler(RoutedEventHandler));
                        }
                    }));
            }
        }

        void RoutedEventHandler(object sender, RoutedEventArgs e)
        {
            if (e.Source == AssociatedObject)
            {
                foreach (var prop in Properties)
                {
                    vsm.Save(GetKey(prop), AssociatedObject.GetValue(prop));
                }
            }
        }

        protected override void OnDetaching()
        {
            if (vsm != null)
            {
                foreach (var ev in Events)
                {
                    AssociatedObject.RemoveHandler(ev, new RoutedEventHandler(RoutedEventHandler));
                }
                vsm = null;
            }
        }

        private void RestoreValues()
        {
            foreach (var prop in Properties)
            {
                object value;
                if (vsm.Get(GetKey(prop), out value))
                {
                    AssociatedObject.SetValue(prop, value);
                }
            }
        }
    }
}
