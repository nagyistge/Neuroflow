using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public abstract class LearningBehavior : LayerBehavior
    {
        public int GroupID { get; set; }

        protected override bool PropsEquals(LayerBehavior other)
        {
            return GroupID == ((LearningBehavior)other).GroupID;
        }

        protected override int GenerateHashCode()
        {
            return GroupID;
        }
    }
}
