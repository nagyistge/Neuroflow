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
using System.ComponentModel;

namespace Neuroflow.Design.ActivityDesigners.Controls
{
    /// <summary>
    /// Interaction logic for PropertyCollectionPresenter.xaml
    /// </summary>
    public partial class PropertyCollectionPresenter : UserControl
    {
        public PropertyCollectionPresenter()
        {
            InitializeComponent();
        }

        public ModelItem ModelItem
        {
            get { return (ModelItem)GetValue(ModelItemProperty); }
            set { SetValue(ModelItemProperty, value); }
        }

        public static readonly DependencyProperty ModelItemProperty =
            DependencyProperty.Register("ModelItem", typeof(ModelItem), typeof(PropertyCollectionPresenter),
                new UIPropertyMetadata((obj, e) => ((PropertyCollectionPresenter)obj).Changed()));

        public IEnumerable<string> PropertyNames
        {
            get { return (IEnumerable<string>)GetValue(PropertyNamesProperty); }
            set { SetValue(PropertyNamesProperty, value); }
        }

        public static readonly DependencyProperty PropertyNamesProperty =
            DependencyProperty.Register("PropertyNames", typeof(IEnumerable<string>), typeof(PropertyCollectionPresenter),
                new UIPropertyMetadata((obj, e) => ((PropertyCollectionPresenter)obj).Changed()));

        public IEnumerable<PresentablePropertyGroup> PresentableGroups
        {
            get { return (IEnumerable<PresentablePropertyGroup>)GetValue(PresentableGroupsProperty); }
            set { SetValue(PresentableGroupsProperty, value); }
        }

        public static readonly DependencyProperty PresentableGroupsProperty =
            DependencyProperty.Register("PresentableGroups", typeof(IEnumerable<PresentablePropertyGroup>), typeof(PropertyCollectionPresenter), new UIPropertyMetadata(null));

        private void Changed()
        {
            var modelItem = ModelItem;
            var propertyNames = PropertyNames;

            if (modelItem != null && propertyNames != null)
            {
                var q = from name in propertyNames
                        let prop = modelItem.Properties[name]
                        where prop != null
                        let groupName = GetGroupName(prop)
                        group name by groupName into g
                        orderby g.Key
                        select new PresentablePropertyGroup
                        {
                            ModelItem = modelItem,
                            Name = g.Key,
                            PropertyNames = g
                        };

                PresentableGroups = q;
            }
            else
            {
                PresentableGroups = null;
            }
        }

        private static string GetGroupName(ModelProperty prop)
        {
            // TODO: Cache this
            
            var category = prop.Attributes.OfType<CategoryAttribute>().FirstOrDefault();

            if (category != null && !string.IsNullOrEmpty(category.Category)) return category.Category;

            return "Misc";
        }
    }
}
