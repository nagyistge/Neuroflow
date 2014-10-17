using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Neuroflow.Networks.Neural;
using Neuroflow.Networks.Neural.Learning;
using System.Collections.ObjectModel;
using Neuroflow.Core;
using Neuroflow.Core.Vectors;

namespace Neuroflow.WFTestConsole
{
    public sealed class CreateStandardMLNetwork : CodeActivity<NeuralNetwork>
    {
        [RequiredArgument]
        public InArgument<Collection<LearningRule>> LearningRules { get; set; }

        [RequiredArgument]
        public InArgument<int> InputInterfaceLength { get; set; }

        [RequiredArgument]
        public InArgument<int> OutputInterfaceLength { get; set; }

        [RequiredArgument]
        public InArgument<int> FirstLayerNeuronCount { get; set; }

        [RequiredArgument]
        public InArgument<int> SecondLayerNeuronCount { get; set; }

        protected override NeuralNetwork Execute(CodeActivityContext context)
        {
            //var rules = LearningRules.Get(context).ToArray();

            var ruleList = new LinkedList<LearningRule>();
            ruleList.AddLast(new NoisedWeightInitializationRule { Noise = 0.05 });
            ruleList.AddLast(
                new SignChangesRule
                {
                    MinStepSize = 0.0000001,
                    MaxStepSize = 0.0005,
                    Momentum = 0.25,
                    IterationRepeat = new IterationRepeatPars(1, 5),
                    Mode = LearningMode.Stochastic
                });
            //ruleList.AddLast(new SCGRule { IterationRepeat = new IterationRepeatPars(5, 10), ScalingMethod = SCGScalingMethod.Moller });
            //ruleList.AddLast(new GradientDescentRule { Momentum = 0.25, StepSize = 0.001, Mode = LearningMode.Batch, IterationRepeat = new IterationRepeatPars(1, 5) });
            var rules = ruleList.ToArray();

            var saf = new SigmoidActivationFunction(1.75);
            var laf = new LinearActivationFunction(1.75);


            var layers = new LinkedList<NeuralLayerDefinition>();

            // First:

            layers.AddLast(new NeuralLayerDefinition(new Factory<NeuralNode>(() => new ActivationNeuron(saf, rules)), FirstLayerNeuronCount.Get(context)));

            // Second:

            layers.AddLast(new NeuralLayerDefinition(new Factory<NeuralNode>(() => new ActivationNeuron(saf, rules)), SecondLayerNeuronCount.Get(context)));

            // Collector:

            layers.AddLast(new NeuralLayerDefinition(new Factory<NeuralNode>(() => new ActivationNeuron(laf, rules)), OutputInterfaceLength.Get(context)));
            
            var a = new StandardMultilayerArchitecture(
                new NeuralConnectionDefinition(new Factory<Synapse>(() => new Synapse(rules)), false),
                InputInterfaceLength.Get(context),
                layers.ToArray());

            var net = a.CreateNetwork();

            return net;
        }
    }
}
