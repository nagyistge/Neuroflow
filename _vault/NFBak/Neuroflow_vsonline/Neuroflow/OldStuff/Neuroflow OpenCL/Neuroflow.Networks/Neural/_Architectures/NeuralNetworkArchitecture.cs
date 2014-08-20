using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Neuroflow.Core.ComponentModel;

namespace Neuroflow.Networks.Neural
{
    public abstract class NeuralNetworkArchitecture
    {
        [Required]
        [Category(PropertyCategories.Structure)]
        [DisplayName("Init Parameters")]
        public NNInitParameters InitParameters { get; set; }

        protected abstract ICollection<ConnectableLayer> CreateLayers();

        protected virtual void Validate()
        {
            if (InitParameters == null) throw new InvalidOperationException("InitParameters is null.");
        }

        public NeuralNetwork CreateNetwork()
        {
            Validate();

            var layers = CreateLayers();

            if (layers == null || layers.Count == 0) throw new InvalidOperationException("Created layer collection is null or empty.");

            var nnType = InitParameters.NeuralNetworkType;

            var constructor = (from ci in nnType.GetConstructors()
                               let pars = ci.GetParameters()
                               where pars.Length == 2 &&
                                  pars[0].ParameterType.IsAssignableFrom(layers.GetType()) &&
                                  pars[1].ParameterType.IsAssignableFrom(InitParameters.GetType())
                               select ci).FirstOrDefault();

            if (constructor == null) throw new InvalidOperationException("Architecture compatible constructor not found on Neural Network type '" + nnType + "'.");

            try
            {
                return (NeuralNetwork)constructor.Invoke(new object[] { layers, InitParameters });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot invoke Architecture compatible constructor on Neural Network type '" + nnType + "'.", ex);
            }
        }
    }
}
