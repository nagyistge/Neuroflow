using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.OpenCL;
using Cloo;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural.OpenCL
{
    public sealed class OpenCLNeuralNetwork : NeuralNetwork, IDisposable
    {
        #region Construct
        
        public OpenCLNeuralNetwork(ICollection<ConnectableLayer> layers)
            : base(layers, new OpenCLNNInitParameters())
        {
            Contract.Requires(layers != null);
            Contract.Requires(layers.Count != 0);
        }

        public OpenCLNeuralNetwork(ICollection<ConnectableLayer> layers, OpenCLNNInitParameters initPars)
            : base(layers, initPars)
        {
            Contract.Requires(layers != null);
            Contract.Requires(layers.Count != 0);
            Contract.Requires(initPars != null);
        } 

        #endregion

        #region Fields

        volatile bool isDisposed;

        IntRange inputBuffer, outputBuffer;

        OpenCLContext oclContext;

        ComputeDevice oclDevice;

        OpenCLQueue oclQueue;

        OpenCLBuffer<float> oclValueBuffer;

        #endregion

        #region Build

        protected override void Build(BufferAllocator allocator, ConnectedLayerGroups connectedLayerGroups, NNInitParameters initPars)
        {
            var oclInitPars = (OpenCLNNInitParameters)initPars;
            CreateOpenCLContext(oclInitPars);
            inputBuffer = connectedLayerGroups.InputBuffer;
            outputBuffer = connectedLayerGroups.OutputBuffer;
        }

        private void CreateOpenCLContext(OpenCLNNInitParameters oclInitPars)
        {
            ComputePlatform platform = null;
            if (String.IsNullOrEmpty(oclInitPars.PlatformName))
            {
                platform = OpenCLContext.DefaultPlatform;
            }
            else
            {
                platform = OpenCLContext.Platforms.Where(p => string.Compare(oclInitPars.PlatformName, p.Name, true) == 0).FirstOrDefault();
            }

            if (platform == null) throw new OpenCLException("Platform '" + oclInitPars.PlatformName + "' not found.");

            oclContext = new OpenCLContext(ComputeDeviceTypes.All, platform);
            try
            {
                if (string.IsNullOrEmpty(oclInitPars.DeviceName))
                {
                    oclDevice = oclContext.DefaultDevice;
                    //oclDevice = oclContext.Devices.Where(d => d.Type == ComputeDeviceTypes.Cpu).First();
                }
                else
                {
                    oclDevice = oclContext.Devices.Where(d => string.Compare(oclInitPars.DeviceName, d.Name, true) == 0).FirstOrDefault();
                }

                if (oclDevice == null) throw new OpenCLException("Device '" + oclInitPars.DeviceName + "' not found.");

                oclQueue = oclContext.CreateQueue(oclDevice, ComputeCommandQueueFlags.OutOfOrderExecution);
            }
            catch
            {
                oclContext.Dispose();
                oclContext = null;
                throw;
            }
        }

        #endregion

        #region After built

        protected override void Built(BufferAllocator allocator, ConnectedLayerGroups connectedLayerGroups, NNInitParameters initPars)
        {
            // Create buffer:
            oclValueBuffer = oclContext.CreateBuffer<float>(allocator.Size, ComputeMemoryFlags.ReadWrite);

            // Fill with zeros: 
            // TODO: Add this stuff to and OpenCLUtils class or sumthin
            int size = 1000;
            int remain = allocator.Size % size;
            float[] zeros = new float[size];

            if (remain != 0) oclQueue.Write(oclValueBuffer, zeros, 0, remain, false);
            for (int i = remain; i < allocator.Size; i += size)
            {
                oclQueue.Write(oclValueBuffer, zeros, i, size, false);
            }

            oclQueue.ComputeCommandQueue.Finish();
        }

        #endregion

        #region I/O

        protected override unsafe void DoWriteInput(float[] values)
        {
            oclQueue.Write(oclValueBuffer, values, inputBuffer.MinValue, inputBuffer.Size); 
        }

        protected override unsafe void DoReadOutput(float[] values)
        {
            oclQueue.Read(oclValueBuffer, values, outputBuffer.MinValue, outputBuffer.Size);
        }

        #endregion

        protected override void InitializeLearningAlgorithms(BufferAllocator allocator, Learning.LearningLayerGroups learningLayerGroups, NNInitParameters initPars)
        {
        }

        protected internal override unsafe void CallBeforeIterationLearningAlgorithms(bool isNewBatch)
        {
        }

        protected internal override unsafe void CallErrorBasedBatchLearningAlgorithms(int batchSize, double averageError)
        {
        }

        protected internal override unsafe void CallErrorBasedStochasticLearningAlgorithms(double averageError)
        {
        }

        protected override unsafe void DoIteration(bool collectTrainingData, int? innerIterationIndex)
        {
        }

        protected override unsafe void DoBackpropagate(BackprogrationMode mode, int? innerIterationIndex = null)
        {
        }

        protected override unsafe void DoPropagatePValues(float[] eVector)
        {
        }

        protected override unsafe void DoWriteError(float[] values)
        {
        }

        protected override unsafe void ResetAlgorithms()
        {
        }

        protected override unsafe void ResetForwardValues(NeuralNetworkResetTarget target)
        {
        }

        protected override unsafe void ResetBackwardValues(NeuralNetworkResetTarget target)
        {
        }

        #region Dispose

        private void DisposeUnmanagedResources()
        {
            oclValueBuffer.Dispose();
            oclValueBuffer = null;
            oclQueue.Dispose();
            oclQueue = null;
            oclContext.Dispose();
            oclContext = null;
        }

        private void DisposeManagedResources()
        {
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().Name);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed) return;
            if (disposing) DisposeManagedResources();
            DisposeUnmanagedResources();
            isDisposed = true;
        }

        ~OpenCLNeuralNetwork()
        {
            Dispose(false);
        }

        #endregion
    }
}
