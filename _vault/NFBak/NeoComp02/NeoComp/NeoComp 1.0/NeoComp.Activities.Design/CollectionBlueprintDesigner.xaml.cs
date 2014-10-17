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
using NeoComp.Activities.Design.Helpers;

namespace NeoComp.Activities.Design
{
    // Interaction logic for CollectionBlueprintDesigner.xaml
    public partial class CollectionBlueprintDesigner
    {
        public CollectionBlueprintDesigner()
        {
            InitializeComponent();
        }

        ActivityArgumentBinder binder;

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);

            if (ModelItem != null)
            {
                binder = new ActivityArgumentBinder(ModelItem);
            }
            else
            {
                binder = null;
            }
        }
    }
}
