using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neuroflow.Data;
using Neuroflow.NeuralNetworks;

namespace Neuroflow
{
    public class ManagedContext : ComputationContext
    {
        public override Device Device
        {
            get { return new Device("Managed", "Managed", ".NET"); }
        }

        ManagedDataArrayFactory daFactory = new ManagedDataArrayFactory();

        public override DataArrayFactory DataArrayFactory
        {
            get { return daFactory; }
        }

        ManagedVectorUtils vectorUtils = new ManagedVectorUtils();

        public override VectorUtils VectorUtils
        {
            get { return vectorUtils; }
        }

        ManagedMultiplayerPerceptronAdapter multilayerPerceptronAdapter = new ManagedMultiplayerPerceptronAdapter();

        protected override IMultilayerPerceptronAdapter MultilayerPerceptronAdapter
        {
            get { return multilayerPerceptronAdapter; }
        }
    }
}
