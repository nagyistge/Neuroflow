using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Networks.Computational.Neural;
using NeoComp.Imaging;
using System.Drawing;
using ImgNoise.Features;
using System.Threading.Tasks;
using System.Diagnostics;
using NeoComp.Computations;

namespace ImgNoise.Test
{
    public sealed class RecurrentNoiseRemoval
    {
        public RecurrentNoiseRemoval(NeuralNetwork network, ImageBuffer source, double noiseLevel)
        {
            Contract.Requires(network != null);
            Contract.Requires(source != null);
            Contract.Requires(noiseLevel >= 0.0 && noiseLevel <= 1.0);

            Network = network;
            VerifyNetwork();
            Source = source;
            NoiseLevel = noiseLevel;
            Dest = new ImageBuffer(source.Width, source.Height);
            rNetwork = Network;
            gNetwork = Network.Clone();
            bNetwork = Network.Clone();
            rReset = new ResetHandler(rNetwork, false);
            gReset = new ResetHandler(gNetwork, false);
            bReset = new ResetHandler(bNetwork, false);
            size2 = Size / 2;
            swComp = new Stopwatch();
            swRemove = new Stopwatch();
            this.noiseLevel = ImgNoise.Features.Helpers.GetNoiseLevel(NoiseLevel);
        }

        NeuralNetwork rNetwork, gNetwork, bNetwork;

        ResetHandler rReset, gReset, bReset;

        int size2;

        Stopwatch swRemove, swComp;

        double noiseLevel;

        public ImageBuffer Source { get; private set; }

        public ImageBuffer Dest { get; private set; }

        public NeuralNetwork Network { get; private set; }

        public double NoiseLevel { get; private set; }

        public int Size
        {
            get { return Network.InputInterface.Length - 1; }
        }

        public TimeSpan LastComputationTime { get; private set; }

        public TimeSpan LastRemoveTime { get; private set; }

        private Color CurrentOutput
        {
            get
            {
                byte r = ImgNoise.Features.Helpers.DoubleToPixel(rNetwork.OutputInterface[0]);
                byte g = ImgNoise.Features.Helpers.DoubleToPixel(gNetwork.OutputInterface[0]);
                byte b = ImgNoise.Features.Helpers.DoubleToPixel(bNetwork.OutputInterface[0]);
                return Color.FromArgb(r, g, b);
            }
        }

        public void RemoveNoise(Action<int> lineProcessedCallback = null)
        {
            swRemove.Reset();
            swComp.Reset();
            swRemove.Start();
            for (int y = 0; y < Source.Height; y++)
            {
                rReset.Reset();
                gReset.Reset();
                bReset.Reset();
                rNetwork.InputInterface[0] = noiseLevel;
                gNetwork.InputInterface[0] = noiseLevel;
                bNetwork.InputInterface[0] = noiseLevel;
                for (int x = 0; x < Source.Width; x++)
                {
                    if (x == 0)
                    {
                        Initialize(y);
                    }
                    else
                    {
                        FeedColumn(x, y);
                    }
                    Dest.SafeSetPixel(x, y, CurrentOutput);
                }
                if (lineProcessedCallback != null) lineProcessedCallback(y);
            }
            swRemove.Stop();
            LastRemoveTime = swRemove.Elapsed;
            LastComputationTime = swComp.Elapsed;
        }

        private void FeedColumn(int col, int row)
        {
            col += size2;
            swComp.Start();
            Parallel.Invoke(
                () => FeedColumn(col, row, rNetwork, Channel.Red),
                () => FeedColumn(col, row, gNetwork, Channel.Green),
                () => FeedColumn(col, row, bNetwork, Channel.Blue));
            swComp.Stop();
        }

        private void Initialize(int row)
        {
            swComp.Start(); 
            Parallel.Invoke(
                () => Initialize(row, rNetwork, Channel.Red),
                () => Initialize(row, gNetwork, Channel.Green),
                () => Initialize(row, bNetwork, Channel.Blue));
            swComp.Stop();
        }

        private void Initialize(int row, NeuralNetwork network, Channel channel)
        {
            for (int x = -size2; x <= size2; x++)
            {
                FeedColumn(x, row, network, channel);
            }
        }

        private void FeedColumn(int col, int row, NeuralNetwork network, Channel channel)
        {
            int inIdx = 1;
            for (int y = row - size2; y <= row + size2; y++, inIdx++)
            {
                Color px = ReadSourceColor(col, y);
                byte pxValue;
                switch (channel)
                {
                    case Channel.Red:
                        pxValue = px.R;
                        break;
                    case Channel.Green:
                        pxValue = px.G;
                        break;
                    default:
                        pxValue = px.B;
                        break;
                }
                network.InputInterface[inIdx] = ImgNoise.Features.Helpers.PixelToDouble(pxValue);
            }
            network.Iteration();
        }

        private Color ReadSourceColor(int x, int y)
        {
            x = CutCoord(Source.Width, x);
            y = CutCoord(Source.Height, y);
            return Source.SafeGetPixel(x, y);
        }

        private static int CutCoord(int size, int pos)
        {
            if (pos < 0) return -pos;
            if (pos >= size) return (2 * size) - pos - 1;
            return pos;
        }

        private void VerifyNetwork()
        {
            if (Network.OutputInterface.Length != 1) throw new InvalidOperationException("Network output interface size must be 1.");
            if (Network.InputInterface.Length <= 1) throw new InvalidOperationException("Network input interface size must be greater than 1.");
        }
    }
}
