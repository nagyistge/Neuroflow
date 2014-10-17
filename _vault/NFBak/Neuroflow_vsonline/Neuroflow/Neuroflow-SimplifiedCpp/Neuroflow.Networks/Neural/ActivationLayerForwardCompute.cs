using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural
{
    public class ActivationLayerForwardCompute : LayerForwardCompute
    {
        #region Constructor

        public ActivationLayerForwardCompute(NeuralNetworkFactory factory, ConnectedLayer connectedLayer)
            : base(factory, connectedLayer)
        {
            Contract.Requires(connectedLayer != null);
            Contract.Requires(connectedLayer.Layer is ActivationLayer);

            FeedForwardOps = factory.CreateFeedForwardOps();
            FeedForwardOps.Initialize(this);
            BPTTOps = factory.CreateBPTTOps();
            BPTTOps.Initialize(this);
            RTLROps = factory.CreateRTLROps();
            RTLROps.Initialize(this);
            Function = ((ActivationLayer)connectedLayer.Layer).Function;
        } 

        #endregion

        #region Properties and Fields

        protected internal IFeedForwardOps FeedForwardOps { get; private set; }

        protected internal IBPTTOps BPTTOps { get; private set; }

        protected internal IRTLROps RTLROps { get; private set; }

        protected internal ActivationFunction Function { get; private set; } 

        #endregion

        #region BW Factory

        protected internal override LayerBackwardCompute CreateBackwardCompute(ConnectedLayer connectedLayer)
        {
            return new ActivationLayerBackwardCompute(this, connectedLayer);
        } 

        #endregion

        #region Compute

        internal override void Compute(NeuralComputationContext context, bool collectTrainingData, int? innerIterationIndex)
        {
            if (collectTrainingData)
            {
                switch (Method)
                {
                    case ForwardComputationMethod.FeedForward:
                        FeedForwardOps.ComputeForward(context);
                        break;
                    case ForwardComputationMethod.BPTT:
                        BPTTOps.ComputeForward(context, innerIterationIndex.Value);
                        break;
                    case ForwardComputationMethod.RTLR:
                        RTLROps.ComputeForward(context);
                        break;
                }
            }
            else
            {
                FeedForwardOps.ComputeForward(context);
            }
        }

        #endregion
    }
}
