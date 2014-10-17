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

namespace Neuroflow.Activities.Design.ActivityDesigners
{
    // Interaction logic for ExperimentDesigner.xaml
    public partial class ExperimentDesigner
    {
        public ExperimentDesigner()
        {
            InitializeComponent();
        }

        ModelItemCollection branchesColl;

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);

            if (ModelItem != null) InitializeDesigning();
        }

        private void InitializeDesigning()
        {
            dynamic item = ModelItem;
            WIP.AllowedItemType = (Type)((ModelItem)item.BranchType).GetCurrentValue();
            branchesColl = ((ModelItemCollection)item.Branches);
            branchesColl.CollectionChanged += OnBranchesCollectionChanged;
            foreach (var branchModelItem in branchesColl) InitializeBranchModelItem(branchModelItem);
        }

        void OnBranchesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (ModelItem bmi in e.NewItems) InitializeBranchModelItem(bmi);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (ModelItem bmi in e.OldItems) UninitializeBranchModelItem(bmi);
            }
            
            if (branchesColl.Count != 0 && !branchesColl.Cast<dynamic>().Any(b => b.IsActive))
            {
                ((dynamic)branchesColl[0]).IsActive = true;
            }            
        }

        void InitializeBranchModelItem(ModelItem branchModelItem)
        {
            branchModelItem.PropertyChanged += OnBranchModelItemPropertyChanged;
        }

        void UninitializeBranchModelItem(ModelItem branchModelItem)
        {
            branchModelItem.PropertyChanged -= OnBranchModelItemPropertyChanged;
        }

        void OnBranchModelItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive" && ((dynamic)sender).IsActive == true)
            {
                foreach (dynamic bmi in branchesColl)
                {
                    if (bmi != sender && bmi.IsActive == true) bmi.IsActive = false;
                }
            }
        }
    }
}
