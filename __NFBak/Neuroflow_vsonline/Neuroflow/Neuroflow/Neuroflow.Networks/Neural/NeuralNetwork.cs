using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics.Contracts;
using Neuroflow.Networks.Neural.Learning;
using System.Collections.ObjectModel;
using Neuroflow.Core.Vectors;
using Neuroflow.Core.Algorithms;

namespace Neuroflow.Networks.Neural
{
    [Flags]
    public enum NNAlgorithm
    {
        None = 0,
        Validation = 1,
        UnorderedTraining = 2,
        StreamedTraining = 4,
        UnorderedTrainingAndValidation = Validation | UnorderedTraining,
        StreamedTrainingAndValidation = Validation | StreamedTraining,
        All = Validation | UnorderedTraining | StreamedTraining
    }
    
    public abstract class NeuralNetwork : Identified, IComputationUnit<float>, ISupportsAlgorithms, IIdentified, IDisposable
    {
        #region Ctor

        protected NeuralNetwork(ICollection<ConnectableLayer> layers, NNInitParameters initPars, NNAlgorithm supportedAlgorithms)
        {
            Contract.Requires(layers != null);
            Contract.Requires(initPars != null);

            if (!initPars.NeuralNetworkType.IsAssignableFrom(GetType())) throw new ArgumentException("Init parameters is not for this type of network.", "parameters");

            RecurrentOptions = initPars.RecurrentOptions;

            this.layers = layers.Select(cl => cl.Layer).ToArray();
            var allocator = new BufferAllocator();

            if (this.layers.OfType<IHasLearningRules>().Any(hlr => hlr.NeedsGradientInformation))
            {
                StructuralElementFlags |= NNStructuralElement.GradientInformation;

                // Needs gradient, but how?
                if (RecurrentOptions != null)
                {
                    //Recurrent:
                    if (RecurrentOptions.Algorithm == RLAlgorithm.BPTT)
                    {
                        // Backprogataion throught time
                        StructuralElementFlags |= NNStructuralElement.BackwardImplementation;
                    }
                    else
                    {
                        // Realtime recurrent learning
                        StructuralElementFlags |= NNStructuralElement.RTLRInformation;
                    }
                }
                else
                {
                    // FF, Backprogatation
                    StructuralElementFlags |= NNStructuralElement.BackwardImplementation;
                }
            }

            var cgs = new ConnectedLayerGroups(allocator, new GroupedLayers(layers), StructuralElementFlags, initPars.RecurrentOptions);
            InputInterfaceLength = cgs.InputBuffer.Size;
            OutputInterfaceLength = cgs.OutputBuffer.Size;
            Build(allocator, cgs, initPars);
            InitializeLearningAlgorithms(allocator, new LearningLayerGroups(cgs), initPars);
            InitializeSupportedAlgorithms(allocator, supportedAlgorithms);
            Built(allocator, cgs, initPars);
        }

        private void InitializeSupportedAlgorithms(BufferAllocator allocator, NNAlgorithm supportedAlgorithms)
        {
            if ((supportedAlgorithms & NNAlgorithm.Validation) != 0) validation = new Validation(this, allocator);
            if ((supportedAlgorithms & NNAlgorithm.UnorderedTraining) != 0) unorderedTraining = new UnorderedTraining(this, allocator);
            if ((supportedAlgorithms & NNAlgorithm.StreamedTraining) != 0) streamedTraining = new StreamedTraining(this, allocator);
        }

        #endregion

        #region Props and Fields

        bool disposed;

        Validation validation;

        StreamedTraining streamedTraining;

        UnorderedTraining unorderedTraining;

        Layer[] layers;

        public ReadOnlyCollection<Layer> Layers
        {
            get { return Array.AsReadOnly(layers); }
        }

        public NNStructuralElement StructuralElementFlags { get; private set; }

        public bool IsBackwardEnabled
        {
            get { return (StructuralElementFlags & NNStructuralElement.BackwardImplementation) != 0; }
        }

        public int InputInterfaceLength { get; private set; }

        public int OutputInterfaceLength { get; private set; }

        public RecurrentOptions RecurrentOptions { get; private set; }

        public bool IsRecurrent
        {
            get { return RecurrentOptions != null; }
        } 

        #endregion

        #region Iterations

        public NeuralComputationContext CreateContext()
        {
            return DoCreateContext();
        }

        public VectorBuffer<float> CreateVectorBuffer()
        {
            return DoCreateVectorBuffer();
        }

        public void Iteration(NeuralComputationContext context)
        {
            Contract.Requires(context != null);

            Iteration(context, false, null);
        }

        public void Iteration(NeuralComputationContext context, bool collectTrainingData = false, int? innerIterationIndex = null)
        {
            Contract.Requires(context != null);
            Contract.Requires(innerIterationIndex == null || (innerIterationIndex.HasValue && RecurrentOptions != null && RecurrentOptions.MaxIterations > innerIterationIndex.Value));

            lock (context.SyncRoot) DoIteration(context, collectTrainingData, innerIterationIndex);
        }

        public void Backpropagate(NeuralComputationContext context, BackprogrationMode mode, int? innerIterationIndex = null)
        {
            Contract.Requires(context != null);
            Contract.Requires(RecurrentOptions == null || (innerIterationIndex.HasValue && RecurrentOptions.MaxIterations > innerIterationIndex.Value));
            Contract.Requires(IsBackwardEnabled);

            lock (context.SyncRoot) DoBackpropagate(context, mode, innerIterationIndex);
        }

        public void WriteInput(NeuralComputationContext context, BufferedVector<float> values)
        {
            Contract.Requires(context != null);
            Contract.Requires(values != null);
            Contract.Requires(values.Length == InputInterfaceLength);

            lock (context.SyncRoot) lock (values.Owner.SyncRoot) DoWriteInput(context, values);
        }

        public void ReadOutput(NeuralComputationContext context, float[] values)
        {
            Contract.Requires(context != null);
            Contract.Requires(values != null);
            Contract.Requires(values.Length == OutputInterfaceLength);

            lock (context.SyncRoot) DoReadOutput(context, values);
        }

        public void Reset(NeuralComputationContext context)
        {
            Contract.Requires(context != null);
            Reset(context, NeuralNetworkResetTarget.All);
        }

        public void Reset(NeuralComputationContext context, NeuralNetworkResetTarget target)
        {
            Contract.Requires(context != null);
            lock (context.SyncRoot)
            {
                if (target == NeuralNetworkResetTarget.All)
                {
                    ResetAll(context);
                }
                else
                {
                    if (((target & NeuralNetworkResetTarget.Outputs) != 0) ||
                        ((target & NeuralNetworkResetTarget.Ps) != 0))
                    {
                        ResetForwardValues(context, target);
                    }

                    if (((target & NeuralNetworkResetTarget.GradientSums) != 0 ||
                         (target & NeuralNetworkResetTarget.Gradients) != 0 ||
                          (target & NeuralNetworkResetTarget.Errors) != 0) &&
                         IsBackwardEnabled)
                    {
                        ResetBackwardValues(context, target);
                    }

                    if ((target & NeuralNetworkResetTarget.Algorithms) != 0)
                    {
                        ResetAlgorithms(context);
                    }
                }
            }
        }

        internal void PropagatePValues(NeuralComputationContext context, IntRange? errorBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires((StructuralElementFlags & NNStructuralElement.RTLRInformation) != 0);
            Contract.Requires(errorBuffer == null || errorBuffer.Value.Size == OutputInterfaceLength);

            lock (context.SyncRoot) DoPropagatePValues(context, errorBuffer);
        }

        internal void ComputeError(NeuralComputationContext context, BufferedVector<float> desiredOutputVector, IntRange errorBuffer, IntRange accumulationBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(desiredOutputVector != null);
            Contract.Requires(desiredOutputVector.Length == errorBuffer.Size);
            Contract.Requires(OutputInterfaceLength == errorBuffer.Size);
            Contract.Requires(accumulationBuffer.Size == OutputInterfaceLength + 1);

            lock (context.SyncRoot) lock (desiredOutputVector.Owner.SyncRoot) DoComputeError(context, desiredOutputVector, errorBuffer, accumulationBuffer);
        }

        internal void SetError(NeuralComputationContext context, IntRange errorBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(OutputInterfaceLength == errorBuffer.Size);

            lock (context.SyncRoot) DoSetError(context, errorBuffer);
        }

        internal void ReadError(NeuralComputationContext context, float[] values, IntRange accumulationBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(values != null);
            Contract.Requires(OutputInterfaceLength == accumulationBuffer.Size - 1);
            Contract.Requires(OutputInterfaceLength == values.Length);

            lock (context.SyncRoot) DoReadError(context, values, accumulationBuffer);
        }

        internal void ZeroBuffer(NeuralComputationContext context, IntRange buffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(buffer.Size == OutputInterfaceLength + 1);

            lock (context.SyncRoot) DoZeroBuffer(context, buffer);
        }

        internal void CalculateAverageError(NeuralComputationContext context, IntRange accumulationBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(accumulationBuffer.Size == OutputInterfaceLength + 1);

            lock (context.SyncRoot) DoCalculateAverageError(context, accumulationBuffer);
        }

        internal void CopyBuffer(NeuralComputationContext context, IntRange source, IntRange target)
        {
            Contract.Requires(context != null);
            Contract.Requires(source.Size == target.Size);

            lock (context.SyncRoot) DoCopyBuffer(context, source, target);
        }

        internal void CallBeforeIterationLearningAlgorithms(NeuralComputationContext context, bool isNewBatch)
        {
            Contract.Requires(context != null);

            lock (context.SyncRoot) DoCallBeforeIterationLearningAlgorithms(context, isNewBatch);
        }

        internal void CallErrorBasedBatchLearningAlgorithms(NeuralComputationContext context, int batchSize, IntRange errorBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(OutputInterfaceLength == errorBuffer.Size);
            Contract.Requires(batchSize > 0);

            lock (context.SyncRoot) DoCallErrorBasedBatchLearningAlgorithms(context, batchSize, errorBuffer);
        }

        internal void CallErrorBasedStochasticLearningAlgorithms(NeuralComputationContext context, IntRange errorBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(OutputInterfaceLength == errorBuffer.Size);

            lock (context.SyncRoot) DoCallErrorBasedStochasticLearningAlgorithms(context, errorBuffer);
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
        }

        ~NeuralNetwork()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                FreeManagedResources();
                GC.SuppressFinalize(this);
                disposed = true;
            }

            FreeUnmanagedResources();
        }

        #endregion

        #region Overridables

        #region Build and Init

        protected abstract void Build(BufferAllocator allocator, ConnectedLayerGroups connectedLayerGroups, NNInitParameters initPars);

        protected abstract void Built(BufferAllocator allocator, ConnectedLayerGroups connectedLayerGroups, NNInitParameters initPars);

        protected abstract void InitializeLearningAlgorithms(BufferAllocator allocator, LearningLayerGroups learningLayerGroups, NNInitParameters initPars);

        #endregion

        #region Iterations

        unsafe protected abstract NeuralComputationContext DoCreateContext();

        unsafe protected abstract VectorBuffer<float> DoCreateVectorBuffer();

        unsafe protected abstract void DoCallBeforeIterationLearningAlgorithms(NeuralComputationContext context, bool isNewBatch);

        unsafe protected abstract void DoCallErrorBasedBatchLearningAlgorithms(NeuralComputationContext context, int batchSize, IntRange errorBuffer);

        unsafe protected abstract void DoCallErrorBasedStochasticLearningAlgorithms(NeuralComputationContext context, IntRange errorBuffer);

        unsafe protected abstract void DoIteration(NeuralComputationContext context, bool collectTrainingData, int? innerIterationIndex);

        unsafe protected abstract void DoBackpropagate(NeuralComputationContext context, BackprogrationMode mode, int? innerIterationIndex = null);

        unsafe protected abstract void DoPropagatePValues(NeuralComputationContext context, IntRange? errorBuffer);

        unsafe protected abstract void DoWriteInput(NeuralComputationContext context, BufferedVector<float> values);

        unsafe protected abstract void DoReadOutput(NeuralComputationContext context, float[] values);

        unsafe protected abstract void ResetAll(NeuralComputationContext context);

        unsafe protected abstract void ResetAlgorithms(NeuralComputationContext context);

        unsafe protected abstract void ResetForwardValues(NeuralComputationContext context, NeuralNetworkResetTarget target);

        unsafe protected abstract void ResetBackwardValues(NeuralComputationContext context, NeuralNetworkResetTarget target);

        unsafe protected abstract void DoComputeError(NeuralComputationContext context, BufferedVector<float> desiredOutputVector, IntRange errorBuffer, IntRange accumulationBuffer);

        unsafe protected abstract void DoSetError(NeuralComputationContext context, IntRange errorBuffer);

        unsafe protected abstract void DoReadError(NeuralComputationContext context, float[] values, IntRange errorBuffer);

        unsafe protected abstract void DoZeroBuffer(NeuralComputationContext context, IntRange accumulationBuffer);

        unsafe protected abstract void DoCalculateAverageError(NeuralComputationContext context, IntRange accumulationBuffer);

        unsafe protected abstract void DoCopyBuffer(NeuralComputationContext context, IntRange source, IntRange target);

        #endregion 

        #region Cleanup

        protected virtual void FreeUnmanagedResources()
        {
        }

        protected virtual void FreeManagedResources()
        {
        }

        #endregion

        #endregion

        #region IComputationUnit<float>

        object IComputationUnit<float>.CreateContext()
        {
            return CreateContext();
        }

        void IComputationUnit<float>.Iteration(object context)
        {
            Iteration((NeuralComputationContext)context);
        }

        void IComputationUnit<float>.WriteInput(object context, BufferedVector<float> values)
        {
            WriteInput((NeuralComputationContext)context, values);
        }

        void IComputationUnit<float>.ReadOutput(object context, float[] values)
        {
            ReadOutput((NeuralComputationContext)context, values);
        }

        void IReset.Reset(object context)
        {
            Reset((NeuralComputationContext)context);
        } 

        #endregion

        #region ISupportsAlgorithms

        public T GetAlgorithm<T>() where T : class
        {
            if (typeof(T) == typeof(Validation) && validation != null) return (T)(object)validation;
            if (typeof(T) == typeof(UnorderedTraining) && unorderedTraining != null) return (T)(object)unorderedTraining;
            if (typeof(T) == typeof(StreamedTraining) && streamedTraining != null) return (T)(object)streamedTraining;

            throw new InvalidOperationException("Algorithm of type '" + typeof(T).Name + "' is not supported by this instance.");
        } 

        #endregion
    }
}
