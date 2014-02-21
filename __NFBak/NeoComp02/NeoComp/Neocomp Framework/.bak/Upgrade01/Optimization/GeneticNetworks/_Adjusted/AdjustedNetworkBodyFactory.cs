using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Optimization.GA;
using NeoComp.Networks;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    [ContractClass(typeof(AdjustedNetworkBodyFactoryContract<,>))]
    public abstract class AdjustedNetworkBodyFactory<TParameters, TNetwork> : SimpleMutatorBodyFactory<double, AdjustedNetworkBody<TNetwork>>
        where TParameters : AdjustedNetworkParameters
        where TNetwork : class, INetwork
    {   
        #region Contructor

        protected AdjustedNetworkBodyFactory(TParameters parameters)
            : base()
        {
            Contract.Requires(parameters != null);

            if (!parameters.CheckIsValid()) throw new ArgumentException("Parameters is not valid.", "parameters");
            Parameters = parameters;
        }

        #endregion
        
        #region Properties

        public TParameters Parameters { get; private set; }

        protected abstract int GeneCount { get; }

        #endregion

        #region Initialization

        protected override IEnumerable<double> CreateInitialGeneSequence()
        {
            return Enumerable.Range(0, GeneCount).Select(idx => RandomGenerator.Random.NextDouble());
        } 

        #endregion

        #region Mutation

        protected override double DoPointMuation(double gene)
        {
            var str = Parameters.MutationStrength;
            if (str.HasValue)
            {
                gene += (RandomGenerator.Random.NextDouble() * 2.0 - 1.0) * str.Value;
                if (gene < 0.0) gene = 0.0; else if (gene > 1.0) gene = 1.0;
                return gene;
            }
            else
            {
                return RandomGenerator.Random.NextDouble();
            }
        } 

        #endregion
    }

    [ContractClassFor(typeof(AdjustedNetworkBodyFactory<,>))]
    abstract class AdjustedNetworkBodyFactoryContract<TParameters, TNetwork> : AdjustedNetworkBodyFactory<TParameters, TNetwork>
        where TParameters : AdjustedNetworkParameters
        where TNetwork : class, INetwork
    {
        public AdjustedNetworkBodyFactoryContract()
            : base(null)
        {
        }
        
        protected override int GeneCount
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return 0;
            }
        }
    }
}
