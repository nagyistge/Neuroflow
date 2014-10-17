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

namespace Neuroflow.Design.ActivityDesigners
{
    // Interaction logic for NewCollectionDesigner.xaml
    public partial class NewCollectionDesigner
    {
        public NewCollectionDesigner()
        {
            InitializeComponent();
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);

            if (ModelItem != null)
            {
                dynamic dmi = ModelItem;
                var at = (ModelItem)dmi.AllowedActivityType;
                WIP.AllowedItemType = (Type)at.GetCurrentValue();
            }
        }
    }
}
