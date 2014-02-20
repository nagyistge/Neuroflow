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

namespace NeoComp.Activities.Design.Controls
{
    /// <summary>
    /// Interaction logic for VariableSetup.xaml
    /// </summary>
    public partial class VariableSetup : UserControl
    {
        public VariableSetup()
        {
            InitializeComponent();
        }

        public string VariableName
        {
            get { return (string)GetValue(VariableNameProperty); }
            set { SetValue(VariableNameProperty, value); }
        }

        public static readonly DependencyProperty VariableNameProperty =
            DependencyProperty.Register("VariableName", typeof(string), typeof(VariableSetup), new UIPropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(VariableSetup), new UIPropertyMetadata(null));
        
    }
}
