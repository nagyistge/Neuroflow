using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;
using System.Diagnostics.Contracts;
using NeoComp.Computations;
using NeoComp.Core;

namespace Gender.Data
{
    public sealed class GenderComputation
    {
        internal const int PicWidth = 20;

        internal const int PicHeight = 24;
        
        #region Constructor

        public GenderComputation(NeuralNetwork network, int numberOfIterations = 1)
        {
            Contract.Requires(network != null);
            Contract.Requires(numberOfIterations > 0);

            Network = network;
            this.binaryOutput = network.OutputInterface.Length == 2;
            NumberOfIterations = numberOfIterations;
            VerifyNetwork();
            if (numberOfIterations != 1) resetHandler = new ResetHandler(Network, false);
        }

        private void VerifyNetwork()
        {
            int inputSize = PicWidth * PicHeight;
            int outputSize = binaryOutput ? 2 : 1;
            if (Network.InputInterface.Length != inputSize) throw new InvalidOperationException("Network Input Interface length size mismatch.");
            if (Network.OutputInterface.Length != outputSize) throw new InvalidOperationException("Network Output Interface length size mismatch.");
        } 

        #endregion

        #region Fields and Properties

        ResetHandler resetHandler;

        bool binaryOutput;

        public NeuralNetwork Network { get; private set; }

        public int NumberOfIterations { get; private set; }

        #endregion

        #region Compute

        public bool ComputeGender(byte[] pixels)
        {
            Contract.Requires(pixels != null);
            Contract.Requires(pixels.Length == PicWidth * PicHeight);

            return GetGender(pixels);
        }

        private bool GetGender(byte[] pixels)
        {
            lock (Network.SyncRoot)
            {
                for (int idx = 0; idx < pixels.Length; idx++) Network.InputInterface[idx] = Helpers.PixelToDouble(pixels[idx]);
                for (int it = 0; it < NumberOfIterations; it++) Network.Iteration();
                return ReadGender();
            }
        }

        private bool ReadGender()
        {
            if (binaryOutput)
            {
                double v0 = Network.OutputInterface[0];
                double v1 = Network.OutputInterface[1];
                return v1 > v0;
            }
            else
            {
                double v = Network.OutputInterface[0];
                return v >= 0.0;
            }
        }

        #endregion
    }
}
