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
using System.Activities;
using System.Collections.ObjectModel;
using Neuroflow.Design.ActivityDesigners.Interface;

namespace Neuroflow.Design.ActivityDesigners.Controls
{
    /// <summary>
    /// Interaction logic for NewObjectActivityPresenter.xaml
    /// </summary>
    public partial class NewObjectActivityPresenter : UserControl
    {
        public NewObjectActivityPresenter()
        {
            InitializeComponent();
        }

        public ModelItem ModelItem
        {
            get { return (ModelItem)GetValue(ModelItemProperty); }
            set { SetValue(ModelItemProperty, value); }
        }

        public static readonly DependencyProperty ModelItemProperty =
            DependencyProperty.Register("ModelItem", typeof(ModelItem), typeof(NewObjectActivityPresenter), 
            new UIPropertyMetadata(null, (obj, e) => ((NewObjectActivityPresenter)obj).OnModelItemPropertyChanged(e)));

        private void OnModelItemPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            var modelItem = (ModelItem)e.NewValue;

            if (modelItem != null)
            {
                InterfaceHelpers.EnsureInitialization(modelItem);

                var propNames = new LinkedList<string>();

                foreach (var prop in modelItem.Properties)
                {
                    if (prop.PropertyType.IsSubclassOf(typeof(ActivityWithResult)))
                    {
                        propNames.AddLast(prop.Name);
                    }
                }

                CollPres.ModelItem = modelItem;
                CollPres.PropertyNames = propNames;
            }
        }
    }
}
