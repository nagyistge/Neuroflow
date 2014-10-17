using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;
using NeoComp.Core;
using NeoComp.Adjustables;

namespace NeoComp.Networks.Neural
{
    public class Synapse : ComputationalConnection<double>, ISynapse
    {
        #region Constructor

        public Synapse()
        {
        }

        public Synapse(double weight)
        {
            Weight = weight;
        } 

        #endregion
        
        #region Properties

        public double Weight { get; set; }

        public double Input
        {
            get { return ConnectionValue.Value; }
        }

        public double Output
        {
            get { return ConnectionValue.Value * Weight; }
        }

        #endregion

        #region IAdjustable Members

        double IAdjustableItem.Adjustment
        {
            get { return Weight; }
            set { Weight = value; }
        }

        #endregion
    }
}
