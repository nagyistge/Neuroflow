using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using NeoComp.Optimization.GA;
using NeoComp.Networks.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class AdjustedNeuralNetworkPopulationFactory
    {
        public AdjustedNeuralNetworkPopulationFactory()
        {
            this.groupCount = 5;
            this.groupSize = 25;
            ChanceOfMigration = 0.1;
            PointMutationChance = 0.05;
            this.mutationStrength = 0.3;
        }

        int groupCount;

        public int GroupCount
        {
            get { return groupCount; }
            set
            {
                Contract.Requires(value > 0);

                groupCount = value;
            }
        }

        int groupSize;

        public int GroupSize
        {
            get { return groupSize; }
            set
            {
                Contract.Requires(value > 0);

                groupSize = value;
            }
        }
        
        public Probability ChanceOfMigration { get; set; }
        
        public Probability PointMutationChance { get; set; }
        
        public IntRange? CrossoverChunkSize { get; set; }

        double? mutationStrength;

        public double? MutationStrength
        {
            get { return mutationStrength; }
            set
            {
                Contract.Requires(!value.HasValue || (value.Value > 0.0 && value.Value <= 1.0));

                mutationStrength = value;
            }
        }
        
        public ISelectionStrategy SelectionStrategy { get; set; }

        public AdjustedNeuralNetworkPopulation Create(NeuralNetwork network, NeuralNetworkTest test)
        {
            Contract.Requires(network != null);
            Contract.Requires(test != null);

            AdjustedNeuralNetworkBodyFactory temp;
            var pop = DoCreate(network, test, out temp);
            return pop;
        }

        public AdjustedNeuralNetworkPopulation Create(NeuralNetwork network, NeuralNetworkTest test, out AdjustedNeuralNetworkBodyFactory bodyFactory)
        {
            Contract.Requires(network != null);
            Contract.Requires(test != null);

            return DoCreate(network, test, out bodyFactory);
        }

        private AdjustedNeuralNetworkPopulation DoCreate(NeuralNetwork network, NeuralNetworkTest test, out AdjustedNeuralNetworkBodyFactory bodyFactory)
        {
            var parameters = new AdjustedTestableNetworkParameters
            {
                Test = test,
                Network = network,
                MutationStrength = MutationStrength
            };

            bodyFactory = new AdjustedNeuralNetworkBodyFactory(parameters);

            if (CrossoverChunkSize.HasValue)
            {
                bodyFactory.CrossoverChunkSize = CrossoverChunkSize.Value;
            }
            else
            {
                bodyFactory.CrossoverChunkSize = IntRange.CreateFixed(parameters.AdjustableItems.Length / 2 - 1);
            }

            bodyFactory.PointMutationChance = PointMutationChance;

            var newPop = new AdjustedNeuralNetworkPopulation();
            newPop.ChanceOfMigration = ChanceOfMigration;

            for (int gIdx = 0; gIdx < groupCount; gIdx++)
            {
                var group = new AdjustedNeuralNetworkGroup(bodyFactory, groupSize);
                if (SelectionStrategy != null) group.SelectionStrategy = SelectionStrategy;
                newPop.Groups.Add(group);
            }

            return newPop;
        }
    }
}
