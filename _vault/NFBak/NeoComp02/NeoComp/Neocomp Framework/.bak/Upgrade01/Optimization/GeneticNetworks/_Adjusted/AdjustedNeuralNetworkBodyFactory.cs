using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class AdjustedNeuralNetworkBodyFactory : AdjustedNetworkBodyFactory<AdjustedTestableNetworkParameters, NeuralNetwork>
    {
        #region Constructor

        public AdjustedNeuralNetworkBodyFactory(AdjustedTestableNetworkParameters parameters)
            : base(parameters)
        {
            Contract.Requires(parameters != null);
        }

        #endregion

        #region Properties

        protected override int GeneCount
        {
            get
            {
                lock (Parameters.SyncRoot)
                {
                    return Parameters.Network.ConnectionCount + Parameters.Network.NodeCount;
                }
            }
        }

        #endregion

        #region Create Body

        protected override AdjustedNetworkBody<NeuralNetwork> CreateBody(NeoComp.Optimization.GA.DNASequence<double> plan)
        {
            return new AdjustedNeuralNetworkBody(plan, Parameters);
        } 

        #endregion
    }
}
