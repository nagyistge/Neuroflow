using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Activities.Presentation.Model;
using NeoComp.Activities.Design.Helpers;
using NeoComp.Activities.Design.ViewModel;

namespace NeoComp.Activities.Design.Controls
{
    /// <summary>
    /// Interaction logic for ActivityDelegatePresenter.xaml
    /// </summary>
    public partial class ActivityDelegatePresenter : UserControl
    {
        public ActivityDelegatePresenter()
        {
            InitializeComponent();
        }

        public bool ShowExpander
        {
            get { return (bool)GetValue(ShowExpanderProperty); }
            set { SetValue(ShowExpanderProperty, value); }
        }

        public static readonly DependencyProperty ShowExpanderProperty =
            DependencyProperty.Register("ShowExpander", typeof(bool), typeof(ActivityDelegatePresenter), new UIPropertyMetadata(true));

        public string ObjectName
        {
            get { return (string)GetValue(ObjectNameProperty); }
            set { SetValue(ObjectNameProperty, value); }
        }

        public static readonly DependencyProperty ObjectNameProperty =
            DependencyProperty.Register("ObjectName", typeof(string), typeof(ActivityDelegatePresenter), new UIPropertyMetadata(null));

        public string Argument1Name
        {
            get { return (string)GetValue(Argument1NameProperty); }
            set { SetValue(Argument1NameProperty, value); }
        }

        public static readonly DependencyProperty Argument1NameProperty =
            DependencyProperty.Register("Argument1Name", typeof(string), typeof(ActivityDelegatePresenter), new UIPropertyMetadata("Argument1"));

        public string Argument2Name
        {
            get { return (string)GetValue(Argument2NameProperty); }
            set { SetValue(Argument2NameProperty, value); }
        }

        public static readonly DependencyProperty Argument2NameProperty =
            DependencyProperty.Register("Argument2Name", typeof(string), typeof(ActivityDelegatePresenter), new UIPropertyMetadata("Argument2"));

        public string Argument3Name
        {
            get { return (string)GetValue(Argument3NameProperty); }
            set { SetValue(Argument3NameProperty, value); }
        }

        public static readonly DependencyProperty Argument3NameProperty =
            DependencyProperty.Register("Argument3Name", typeof(string), typeof(ActivityDelegatePresenter), new UIPropertyMetadata("Argument3"));

        public string Argument4Name
        {
            get { return (string)GetValue(Argument4NameProperty); }
            set { SetValue(Argument4NameProperty, value); }
        }

        public static readonly DependencyProperty Argument4NameProperty =
            DependencyProperty.Register("Argument4Name", typeof(string), typeof(ActivityDelegatePresenter), new UIPropertyMetadata("Argument4"));

        public string Argument5Name
        {
            get { return (string)GetValue(Argument5NameProperty); }
            set { SetValue(Argument5NameProperty, value); }
        }

        public static readonly DependencyProperty Argument5NameProperty =
            DependencyProperty.Register("Argument5Name", typeof(string), typeof(ActivityDelegatePresenter), new UIPropertyMetadata("Argument5"));

        public string Argument6Name
        {
            get { return (string)GetValue(Argument6NameProperty); }
            set { SetValue(Argument6NameProperty, value); }
        }

        public static readonly DependencyProperty Argument6NameProperty =
            DependencyProperty.Register("Argument6Name", typeof(string), typeof(ActivityDelegatePresenter), new UIPropertyMetadata("Argument6"));

        public string Argument7Name
        {
            get { return (string)GetValue(Argument7NameProperty); }
            set { SetValue(Argument7NameProperty, value); }
        }

        public static readonly DependencyProperty Argument7NameProperty =
            DependencyProperty.Register("Argument7Name", typeof(string), typeof(ActivityDelegatePresenter), new UIPropertyMetadata("Argument7"));

        public string Argument8Name
        {
            get { return (string)GetValue(Argument8NameProperty); }
            set { SetValue(Argument8NameProperty, value); }
        }

        public static readonly DependencyProperty Argument8NameProperty =
            DependencyProperty.Register("Argument8Name", typeof(string), typeof(ActivityDelegatePresenter), new UIPropertyMetadata("Argument8"));

        public string ResultName
        {
            get { return (string)GetValue(ResultNameProperty); }
            set { SetValue(ResultNameProperty, value); }
        }

        public static readonly DependencyProperty ResultNameProperty =
            DependencyProperty.Register("ResultName", typeof(string), typeof(ActivityDelegatePresenter), new UIPropertyMetadata("Result"));

        public Visibility Argument1Visible
        {
            get { return (Visibility)GetValue(Argument1VisibleProperty); }
            private set { SetValue(Argument1VisiblePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey Argument1VisiblePropertyKey =
            DependencyProperty.RegisterReadOnly("Argument1Visible", typeof(Visibility), typeof(ActivityDelegatePresenter), new UIPropertyMetadata(Visibility.Collapsed));

        public static DependencyProperty Argument1VisibleProperty = Argument1VisiblePropertyKey.DependencyProperty;

        public Visibility Argument2Visible
        {
            get { return (Visibility)GetValue(Argument2VisibleProperty); }
            private set { SetValue(Argument2VisiblePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey Argument2VisiblePropertyKey =
            DependencyProperty.RegisterReadOnly("Argument2Visible", typeof(Visibility), typeof(ActivityDelegatePresenter), new UIPropertyMetadata(Visibility.Collapsed));

        public static DependencyProperty Argument2VisibleProperty = Argument2VisiblePropertyKey.DependencyProperty;

        public Visibility Argument3Visible
        {
            get { return (Visibility)GetValue(Argument3VisibleProperty); }
            private set { SetValue(Argument3VisiblePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey Argument3VisiblePropertyKey =
            DependencyProperty.RegisterReadOnly("Argument3Visible", typeof(Visibility), typeof(ActivityDelegatePresenter), new UIPropertyMetadata(Visibility.Collapsed));

        public static DependencyProperty Argument3VisibleProperty = Argument3VisiblePropertyKey.DependencyProperty;

        public Visibility Argument4Visible
        {
            get { return (Visibility)GetValue(Argument4VisibleProperty); }
            private set { SetValue(Argument4VisiblePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey Argument4VisiblePropertyKey =
            DependencyProperty.RegisterReadOnly("Argument4Visible", typeof(Visibility), typeof(ActivityDelegatePresenter), new UIPropertyMetadata(Visibility.Collapsed));

        public static DependencyProperty Argument4VisibleProperty = Argument4VisiblePropertyKey.DependencyProperty;

        public Visibility Argument5Visible
        {
            get { return (Visibility)GetValue(Argument5VisibleProperty); }
            private set { SetValue(Argument5VisiblePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey Argument5VisiblePropertyKey =
            DependencyProperty.RegisterReadOnly("Argument5Visible", typeof(Visibility), typeof(ActivityDelegatePresenter), new UIPropertyMetadata(Visibility.Collapsed));

        public static DependencyProperty Argument5VisibleProperty = Argument5VisiblePropertyKey.DependencyProperty;

        public Visibility Argument6Visible
        {
            get { return (Visibility)GetValue(Argument6VisibleProperty); }
            private set { SetValue(Argument6VisiblePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey Argument6VisiblePropertyKey =
            DependencyProperty.RegisterReadOnly("Argument6Visible", typeof(Visibility), typeof(ActivityDelegatePresenter), new UIPropertyMetadata(Visibility.Collapsed));

        public static DependencyProperty Argument6VisibleProperty = Argument6VisiblePropertyKey.DependencyProperty;

        public Visibility Argument7Visible
        {
            get { return (Visibility)GetValue(Argument7VisibleProperty); }
            private set { SetValue(Argument7VisiblePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey Argument7VisiblePropertyKey =
            DependencyProperty.RegisterReadOnly("Argument7Visible", typeof(Visibility), typeof(ActivityDelegatePresenter), new UIPropertyMetadata(Visibility.Collapsed));

        public static DependencyProperty Argument7VisibleProperty = Argument7VisiblePropertyKey.DependencyProperty;

        public Visibility Argument8Visible
        {
            get { return (Visibility)GetValue(Argument8VisibleProperty); }
            private set { SetValue(Argument8VisiblePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey Argument8VisiblePropertyKey =
            DependencyProperty.RegisterReadOnly("Argument8Visible", typeof(Visibility), typeof(ActivityDelegatePresenter), new UIPropertyMetadata(Visibility.Collapsed));

        public static DependencyProperty Argument8VisibleProperty = Argument8VisiblePropertyKey.DependencyProperty;

        public Visibility ResultVisible
        {
            get { return (Visibility)GetValue(ResultVisibleProperty); }
            private set { SetValue(ResultVisiblePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey ResultVisiblePropertyKey =
            DependencyProperty.RegisterReadOnly("ResultVisible", typeof(Visibility), typeof(ActivityDelegatePresenter), new UIPropertyMetadata(Visibility.Collapsed));

        public static DependencyProperty ResultVisibleProperty = ResultVisiblePropertyKey.DependencyProperty;

        public ModelItem ActivityDelegate
        {
            get { return (ModelItem)GetValue(ActivityDelegateProperty); }
            set { SetValue(ActivityDelegateProperty, value); }
        }

        public static readonly DependencyProperty ActivityDelegateProperty =
            DependencyProperty.Register("ActivityDelegate", typeof(ModelItem), typeof(ActivityDelegatePresenter),
                new UIPropertyMetadata(null, (obj, e) => ((ActivityDelegatePresenter)obj).OnActivityDelegatePropertyChanged(e)));


        private void OnActivityDelegatePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            var ad = ActivityDelegate;
            if (ad != null)
            {
                ResultVisible = (ActivityDelegate.Properties["Result"] != null) ? Visibility.Visible : Visibility.Collapsed;
                Argument1Visible = (ActivityDelegate.Properties["Argument"] != null || ActivityDelegate.Properties["Argument1"] != null) ? Visibility.Visible : Visibility.Collapsed;
                Argument2Visible = (ActivityDelegate.Properties["Argument2"] != null) ? Visibility.Visible : Visibility.Collapsed;
                Argument3Visible = (ActivityDelegate.Properties["Argument3"] != null) ? Visibility.Visible : Visibility.Collapsed;
                Argument4Visible = (ActivityDelegate.Properties["Argument4"] != null) ? Visibility.Visible : Visibility.Collapsed;
                Argument5Visible = (ActivityDelegate.Properties["Argument5"] != null) ? Visibility.Visible : Visibility.Collapsed;
                Argument6Visible = (ActivityDelegate.Properties["Argument6"] != null) ? Visibility.Visible : Visibility.Collapsed;
                Argument7Visible = (ActivityDelegate.Properties["Argument7"] != null) ? Visibility.Visible : Visibility.Collapsed;
                Argument8Visible = (ActivityDelegate.Properties["Argument8"] != null) ? Visibility.Visible : Visibility.Collapsed;

                var item = new ActivityDelegateItem(ad);
                
                ObjectName = item.ObjectName;
                if (!string.IsNullOrEmpty(item.Argument1Name)) Argument1Name = item.Argument1Name;
                if (!string.IsNullOrEmpty(item.Argument2Name)) Argument2Name = item.Argument2Name;
                if (!string.IsNullOrEmpty(item.Argument3Name)) Argument3Name = item.Argument3Name;
                if (!string.IsNullOrEmpty(item.Argument4Name)) Argument4Name = item.Argument4Name;
                if (!string.IsNullOrEmpty(item.Argument5Name)) Argument5Name = item.Argument5Name;
                if (!string.IsNullOrEmpty(item.Argument6Name)) Argument6Name = item.Argument6Name;
                if (!string.IsNullOrEmpty(item.Argument7Name)) Argument7Name = item.Argument7Name;
                if (!string.IsNullOrEmpty(item.Argument8Name)) Argument8Name = item.Argument8Name;
            }
            else
            {
                ResultVisible = Visibility.Collapsed;
                Argument1Visible = Visibility.Collapsed;
                Argument2Visible = Visibility.Collapsed;
                Argument3Visible = Visibility.Collapsed;
                Argument4Visible = Visibility.Collapsed;
                Argument5Visible = Visibility.Collapsed;
                Argument6Visible = Visibility.Collapsed;
                Argument7Visible = Visibility.Collapsed;
                Argument8Visible = Visibility.Collapsed;
            }
        }
    }
}
