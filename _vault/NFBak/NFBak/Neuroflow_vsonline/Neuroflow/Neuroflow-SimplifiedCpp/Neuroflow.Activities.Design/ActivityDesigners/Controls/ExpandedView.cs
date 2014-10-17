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

namespace Neuroflow.Activities.Design.ActivityDesigners.Controls
{
    public class ExpandedView : ContentControl
    {
        static ExpandedView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpandedView), new FrameworkPropertyMetadata(typeof(ExpandedView)));
        }

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(ExpandedView), new UIPropertyMetadata(true));
    }
}
