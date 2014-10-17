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
using System.Activities.Presentation.View;
using Neuroflow.Activities.Design.ActivityDesigners.ViewState;

namespace Neuroflow.Activities.Design.ActivityDesigners.Controls
{
    /// <summary>
    /// Interaction logic for PropertyGroupPeresenter.xaml
    /// </summary>
    public partial class PropertyGroupPresenter : UserControl
    {
        public PropertyGroupPresenter()
        {
            InitializeComponent();
        }

        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(string), typeof(PropertyGroupPresenter),
                new UIPropertyMetadata((obj, e) => ((PropertyGroupPresenter)obj).GroupNameChanged(e)));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(PropertyGroupPresenter),
                new UIPropertyMetadata(true, (obj, e) => ((PropertyGroupPresenter)obj).IsExpandedChanged(e)));

        public ModelItem ModelItem
        {
            get { return (ModelItem)GetValue(ModelItemProperty); }
            set { SetValue(ModelItemProperty, value); }
        }

        public static readonly DependencyProperty ModelItemProperty =
            DependencyProperty.Register("ModelItem", typeof(ModelItem), typeof(PropertyGroupPresenter),
                new UIPropertyMetadata((obj, e) => ((PropertyGroupPresenter)obj).Changed()));

        public IEnumerable<string> PropertyNames
        {
            get { return (IEnumerable<string>)GetValue(PropertyNamesProperty); }
            set { SetValue(PropertyNamesProperty, value); }
        }

        public static readonly DependencyProperty PropertyNamesProperty =
            DependencyProperty.Register("PropertyNames", typeof(IEnumerable<string>), typeof(PropertyGroupPresenter),
                new UIPropertyMetadata((obj, e) => ((PropertyGroupPresenter)obj).Changed()));

        public IEnumerable<PresentableProperty> PresentableProperties
        {
            get { return (IEnumerable<PresentableProperty>)GetValue(PresentablePropertiesProperty); }
            set { SetValue(PresentablePropertiesProperty, value); }
        }

        public static readonly DependencyProperty PresentablePropertiesProperty =
            DependencyProperty.Register("PresentableProperties", typeof(IEnumerable<PresentableProperty>), typeof(PropertyGroupPresenter), new UIPropertyMetadata(null));

        private void Changed()
        {
            var modelItem = ModelItem;
            var propertyNames = PropertyNames;

            if (modelItem != null && propertyNames != null)
            {
                PresentableProperties = propertyNames.OrderBy(n => n).Select(n => new PresentableProperty { ModelItem = modelItem, PropertyName = n });
            }
            else
            {
                PresentableProperties = null;
            }
        }

        bool expandedHandlerDisabled;

        void IsExpandedChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(GroupName) && !expandedHandlerDisabled)
            {
                var viewRoot = this.FindWorkflowViewElement();
                var modelItem = viewRoot.ModelItem;
                var vss = viewRoot.FindViewStateService();
                vss.StoreViewState(modelItem, GetVSSKey(GroupName), e.NewValue);
            }
        }

        bool vssLoaded;

        private void GroupNameChanged(DependencyPropertyChangedEventArgs e)
        {
            var viewRoot = this.FindWorkflowViewElement();
            var modelItem = viewRoot.ModelItem;
            var vss = viewRoot.FindViewStateService();
            if (vssLoaded)
            {
                string oldV = e.OldValue as string;
                string newV = e.NewValue as string;
                if (!string.IsNullOrEmpty(oldV)) vss.RemoveViewState(modelItem, GetVSSKey(oldV));
                if (!string.IsNullOrEmpty(newV)) vss.StoreViewState(modelItem, GetVSSKey(newV), e.NewValue);
            }
            else
            {
                string newV = e.NewValue as string;
                if (!string.IsNullOrEmpty(newV))
                {
                    bool? isExp = vss.RetrieveViewState(modelItem, GetVSSKey(newV)) as bool?;
                    if (isExp != null)
                    {
                        expandedHandlerDisabled = true;
                        try
                        {
                            IsExpanded = isExp.Value;
                        }
                        finally
                        {
                            expandedHandlerDisabled = false;
                        }
                    }
                }
                vssLoaded = true;
            }
        }

        private string GetVSSKey(string groupName)
        {
            return groupName + ".IsExpanded";
        }
    }
}
