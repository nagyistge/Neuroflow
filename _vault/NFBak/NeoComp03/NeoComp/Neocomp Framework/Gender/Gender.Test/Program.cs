using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;
using System.Xml;
using System.Runtime.Serialization;
using Gender.Data;

namespace Gender.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var network = LoadNetwork("comp.xml");
            var comp = new GenderComputation(network, 1);
            var test = new GenderComputationTester(comp, GenderTestSet.Validation);
            test.Update();
            Console.WriteLine("Count: {0} Passed: {1} Failed: {2}", test.TestedItems, test.PassedItems, test.FailedItems);
            Console.WriteLine("Ratio: {0}", Math.Round(test.PassedRatio * 100.0, 4));
            Console.ReadKey();
        }

        private static NeuralNetwork LoadNetwork(string path)
        {
            using (var xml = XmlReader.Create(path))
            {
                var s = new DataContractSerializer(typeof(NeuralNetwork));
                return (NeuralNetwork)s.ReadObject(xml);
            }
        }
    }
}
