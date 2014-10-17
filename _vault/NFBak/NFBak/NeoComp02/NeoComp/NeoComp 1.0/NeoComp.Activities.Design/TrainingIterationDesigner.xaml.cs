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
using System.Activities.Presentation.Model;

namespace NeoComp.Activities.Design
{
    // Interaction logic for TrainingIterationDesigner.xaml
    public partial class TrainingIterationDesigner
    {
        public TrainingIterationDesigner()
        {
            InitializeComponent();
        }

        ActivityArgumentBinder argBinder;

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            var mi = newItem as ModelItem;
            if (mi != null)
            {
                argBinder = new ActivityArgumentBinder(mi);
            }
            else
            {
                argBinder = null;
            }
        }
    }
}
