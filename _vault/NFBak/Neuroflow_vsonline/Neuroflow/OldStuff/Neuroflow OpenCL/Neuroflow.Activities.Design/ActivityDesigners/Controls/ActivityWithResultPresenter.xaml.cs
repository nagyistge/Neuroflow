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
using System.Activities.Presentation;
using Neuroflow.Activities.Design.ActivityDesigners.Converters;
using System.Activities.Presentation.View;
using System.ComponentModel;

namespace Neuroflow.Activities.Design.ActivityDesigners.Controls
{
    /// <summary>
    /// Interaction logic for ActivityWithResultPresenter.xaml
    /// </summary>
    public partial class ActivityWithResultPresenter : UserControl
    {
        public ActivityWithResultPresenter()
        {
            InitializeComponent();
        }

        public ModelItem ModelItem
        {
            get { return (ModelItem)GetValue(ModelItemProperty); }
            set { SetValue(ModelItemProperty, value); }
        }

        public static readonly DependencyProperty ModelItemProperty =
            DependencyProperty.Register("ModelItem", typeof(ModelItem), typeof(ActivityWithResultPresenter),
                new UIPropertyMetadata((obj, e) => ((ActivityWithResultPresenter)obj).Changed()));

        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(ActivityWithResultPresenter),
                new UIPropertyMetadata((obj, e) => ((ActivityWithResultPresenter)obj).Changed()));

        public string PropertyDisplayName
        {
            get { return (string)GetValue(PropertyDisplayNameProperty); }
            set { SetValue(PropertyDisplayNameProperty, value); }
        }

        public static readonly DependencyProperty PropertyDisplayNameProperty =
            DependencyProperty.Register("PropertyDisplayName", typeof(string), typeof(ActivityWithResultPresenter), new UIPropertyMetadata(null));

        private void Changed()
        {
            var modelItem = ModelItem;
            string propertyName = PropertyName;

            if (modelItem != null && !string.IsNullOrEmpty(propertyName))
            {
                var modProp = modelItem.Properties[propertyName];

                if (modProp != null)
                {
                    var binding = new Binding
                    {
                        Source = modelItem,
                        Path = new PropertyPath(propertyName),
                        Mode = BindingMode.TwoWay
                    };

                    WP.SetBinding(WorkflowItemPresenter.ItemProperty, binding);
                    WP.AllowedItemType = modProp.PropertyType;
                    ExpBox.SetBinding(ExpressionTextBox.ExpressionProperty, binding);
                    ExpBox.OwnerActivity = modelItem;

                    var dna = modProp.Attributes.OfType<DisplayNameAttribute>().Where(a => !string.IsNullOrEmpty(a.DisplayName)).FirstOrDefault();
                    PropertyDisplayName = dna != null ? dna.DisplayName : propertyName;
                }
            }
        }
    }
}
