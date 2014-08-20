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
using System.Diagnostics;

namespace Neuroflow.Networks.Neural
{
    public class NeuralNetwork : Identified, IComputationUnit<float>, ISupportsAlgorithms, IIdentified, IDisposable
    {
        #region Ctor

        protected NeuralNetwork(ICollection<ConnectableLayer> layers, NeuralNetworkFactory factory, NNAlgorithm supportedAlgorithms)
        {
            Contract.Requires(layers != null);
            Contract.Requires(factory != null);

            this.factory = factory;
            bufferOps = factory.CreateBufferOps();

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

            var cgs = new ConnectedLayerGroups(allocator, new GroupedLayers(layers), StructuralElementFlags, factory.RecurrentOptions);
            InputInterfaceLength = cgs.InputBuffer.Size;
            OutputInterfaceLength = cgs.OutputBuffer.Size;
            Build(allocator, cgs);
            InitializeLearningAlgorithms(allocator, new LearningLayerGroups(cgs));
            InitializeSupportedAlgorithms(allocator, supportedAlgorithms);
            Built(allocator, cgs);
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

        NeuralNetworkFactory factory;

        IBufferOps bufferOps;

        Validation validation;

        StreamedTraining streamedTraining;

        UnorderedTraining unorderedTraining;

        Layer[] layers;

        int allocatedBuffSize;

        IntRange inputBuffer;

        IntRange outputBuffer;

        int avgValueIndex;

        LayerForwardCompute[][] forwardComputeGroups;

        LayerBackwardCompute[][] backwardComputeGroups;

        LearningAlgorithm[] algorithms, beforeIterationAlgorithms, errorBasedAlgorithms;

        PValuePropagator pValProp;

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

        public RecurrentOptions RecurrentOptions
        {
            get { return factory.RecurrentOptions; }
        }

        public bool IsRecurrent
        {
            get { return RecurrentOptions != null; }
        } 

        #endregion

        #region Build

        #region Entry

        private void Build(BufferAllocator allocator, ConnectedLayerGroups connectedLayerGroups)
        {
            inputBuffer = connectedLayerGroups.InputBuffer;
            outputBuffer = connectedLayerGroups.OutputBuffer;

            BuildForwardComputation(allocator, connectedLayerGroups);
            if (IsBackwardEnabled) BuildBackwardComputation(connectedLayerGroups);
        }

        #endregion

        #region Forward

        private void BuildForwardComputation(BufferAllocator allocator, ConnectedLayerGroups connectedLayerGroups)
        {
            forwardComputeGroups = new LayerForwardCompute[connectedLayerGroups.Groups.Count][];
            for (int groupIndex = 0; groupIndex < connectedLayerGroups.Groups.Count; groupIndex++)
            {
                var group = connectedLayerGroups.Groups[groupIndex];
                forwardComputeGroups[groupIndex] = new LayerForwardCompute[group.Count];
                for (int layerIndex = 0; layerIndex < group.Count; layerIndex++)
                {
                    forwardComputeGroups[groupIndex][layerIndex] = CreateLayerForwardCompute(group[layerIndex]);
                }
            }
        }

        private LayerForwardCompute CreateLayerForwardCompute(ConnectedLayer clayer)
        {
            LayerForwardCompute result = null;
            if (clayer.Layer is ActivationLayer)
            {
                result = new ActivationLayerForwardCompute(clayer);
            }

            if (result == null) throw new InvalidOperationException("Cannot build Managed Neural Network, because '" + clayer.Layer.GetType().FullName + "' layer type is unknown.");

            return result;
        }

        #endregion

        #region Backward

        private void BuildBackwardComputation(ConnectedLayerGroups connectedLayerGroups)
        {
            backwardComputeGroups = new LayerBackwardCompute[forwardComputeGroups.Length][];
            for (int groupIndex = 0; groupIndex < forwardComputeGroups.Length; groupIndex++)
            {
                var connLayerGroup = connectedLayerGroups.Groups[(connectedLayerGroups.Groups.Count - 1) - groupIndex];
                var forwardGroup = forwardComputeGroups[(forwardComputeGroups.Length - 1) - groupIndex];
                backwardComputeGroups[groupIndex] = new LayerBackwardCompute[forwardGroup.Length];
                for (int layerIndex = 0; layerIndex < forwardGroup.Length; layerIndex++)
                {
                    backwardComputeGroups[groupIndex][layerIndex] = forwardGroup[layerIndex].CreateBackwardCompute(connLayerGroup[layerIndex]);
                }
            }
        }

        #endregion

        #region After Built

        protected override void Built(BufferAllocator allocator, ConnectedLayerGroups connectedLayerGroups)
        {
            // Store buff size:
            allocatedBuffSize = allocator.Size;

            // RTLR:
            if ((StructuralElementFlags & NNStructuralElement.RTLRInformation) != 0)
            {
                pValProp = new PValuePropagator(connectedLayerGroups.IndexTable, forwardComputeGroups.SelectMany(g => g));
            }
        }

        #endregion

        #endregion

        #region Iteration

        public NeuralComputationContext CreateContext()
        {
            return factory.CreateContext();
        }

        public VectorBuffer<float> CreateVectorBuffer()
        {
            return factory.CreateVectorBuffer();
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

            lock (context.SyncRoot)
            {
                foreach (var group in forwardComputeGroups)
                {
                    for (int layerIndex = 0; layerIndex < group.Length; layerIndex++)
                    {
                        group[layerIndex].Compute(context, collectTrainingData, innerIterationIndex);
                    }
                }
            }
        }

        #endregion

        #region Backpropagate

        public void Backpropagate(NeuralComputationContext context, BackprogrationMode mode, int? innerIterationIndex = null)
        {
            Contract.Requires(context != null);
            Contract.Requires(RecurrentOptions == null || (innerIterationIndex.HasValue && RecurrentOptions.MaxIterations > innerIterationIndex.Value));
            Contract.Requires(IsBackwardEnabled);

            lock (context.SyncRoot)
            {
                foreach (var group in backwardComputeGroups)
                {
                    for (int layerIndex = 0; layerIndex < group.Length; layerIndex++)
                    {
                        group[layerIndex].Compute(context, mode, innerIterationIndex);
                    }
                }
            }
        }

        #endregion

        #region RTLR

        internal void PropagatePValues(NeuralComputationContext context, IntRange? errorBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires((StructuralElementFlags & NNStructuralElement.RTLRInformation) != 0);
            Contract.Requires(errorBuffer == null || errorBuffer.Value.Size == OutputInterfaceLength);

            lock (context.SyncRoot)
            {
                Debug.Assert(pValProp != null);

                pValProp.Propagate(context, errorBuffer);
            }
        }

        #endregion

        #region Write and Read

        public void WriteInput(NeuralComputationContext context, BufferedVector<float> values)
        {
            Contract.Requires(context != null);
            Contract.Requires(values != null);
            Contract.Requires(values.Length == InputInterfaceLength);

            lock (context.SyncRoot)
            {
                lock (values.Owner.SyncRoot)
                {
                    bufferOps.WriteVector(context, inputBuffer, values);
                }
            }
        }

        public void ReadOutput(NeuralComputationContext context, float[] values)
        {
            Contract.Requires(context != null);
            Contract.Requires(values != null);
            Contract.Requires(values.Length == OutputInterfaceLength);

            lock (context.SyncRoot)
            {
                bufferOps.ReadArray(context, outputBuffer, values);
            }
        }

        internal void ComputeError(NeuralComputationContext context, BufferedVector<float> desiredOutputVector, IntRange errorBuffer, IntRange accumulationBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(desiredOutputVector != null);
            Contract.Requires(desiredOutputVector.Length == errorBuffer.Size);
            Contract.Requires(OutputInterfaceLength == errorBuffer.Size);
            Contract.Requires(accumulationBuffer.Size == OutputInterfaceLength + 1);

            lock (context.SyncRoot)
            {
                lock (desiredOutputVector.Owner.SyncRoot)
                {
                    bufferOps.ComputeError(context, outputBuffer, desiredOutputVector, errorBuffer, accumulationBuffer);
                }
            }
        }

        internal void SetError(NeuralComputationContext context, IntRange errorBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(OutputInterfaceLength == errorBuffer.Size);

            lock (context.SyncRoot)
            {
                backwardComputeGroups[0][0].SetError(context, errorBuffer);
            }
        }

        internal void ReadError(NeuralComputationContext context, float[] values, IntRange accumulationBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(values != null);
            Contract.Requires(OutputInterfaceLength == accumulationBuffer.Size - 1);
            Contract.Requires(OutputInterfaceLength == values.Length);

            lock (context.SyncRoot)
            {
                bufferOps.ReadArray(context, accumulationBuffer, values);
            }
        }

        internal void ZeroBuffer(NeuralComputationContext context, IntRange buffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(buffer.Size == OutputInterfaceLength + 1);

            lock (context.SyncRoot)
            {
                bufferOps.Zero(context, buffer);
            }
        }

        internal void CalculateAverageError(NeuralComputationContext context, IntRange accumulationBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(accumulationBuffer.Size == OutputInterfaceLength + 1);

            lock (context.SyncRoot)
            {
                bufferOps.CalculateAverageError(context, accumulationBuffer);
            }
        }

        internal void CopyBuffer(NeuralComputationContext context, IntRange source, IntRange target)
        {
            Contract.Requires(context != null);
            Contract.Requires(source.Size == target.Size);

            lock (context.SyncRoot)
            {
                bufferOps.CopyBuffer(context, source, target);
            }
        }

        #endregion

        #region Reset

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
                    bufferOps.ZeroAll(context);
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

        private void ResetAlgorithms(NeuralComputationContext context)
        {
            foreach (var algo in algorithms) algo.InitializeNewRun(context);
        }

        private void ResetForwardValues(NeuralComputationContext context, NeuralNetworkResetTarget target)
        {
            bufferOps.Zero(context, inputBuffer);

            for (int groupIndex = 0; groupIndex < forwardComputeGroups.Length; groupIndex++)
            {
                var compute = forwardComputeGroups[groupIndex];
                for (int layerIndex = 0; layerIndex < compute.Length; layerIndex++)
                {
                    compute[layerIndex].Reset(context, target);
                }
            }
        }

        private void ResetBackwardValues(NeuralComputationContext context, NeuralNetworkResetTarget target)
        {
            for (int groupIndex = 0; groupIndex < backwardComputeGroups.Length; groupIndex++)
            {
                var compute = backwardComputeGroups[groupIndex];
                for (int layerIndex = 0; layerIndex < compute.Length; layerIndex++)
                {
                    compute[layerIndex].Reset(context, target);
                }
            }
        }

        #endregion

        #region Algorithms

        #region Init

        protected override void InitializeLearningAlgorithms(BufferAllocator allocator, LearningLayerGroups learningLayerGroups)
        {
            algorithms = new LearningAlgorithm[learningLayerGroups.Count];
            var biAlgos = new LinkedList<LearningAlgorithm>();
            var aeAlgos = new LinkedList<LearningAlgorithm>();

            int idx = 0;
            foreach (var group in learningLayerGroups)
            {
                var algo = CreateAlgorithmForRule(group.Rule);
                algo.InitializeAlgo(allocator, group.Rule, group.ConnectedLayers.ToArray());
                algorithms[idx++] = algo;
                if (algo.Rule.IsBeforeIterationRule) biAlgos.AddLast(algo);
                if (algo.Rule.IsErrorBasedRule) aeAlgos.AddLast(algo);
            }

            beforeIterationAlgorithms = biAlgos.ToArray();
            errorBasedAlgorithms = aeAlgos.ToArray();

            if (errorBasedAlgorithms.Length != 0)
            {
                avgValueIndex = allocator.Alloc(1).MinValue;
            }
        }

        private LearningAlgorithm CreateAlgorithmForRule(LearningRule rule)
        {
            LearningAlgorithm algo = null;

            if (rule is NoisedWeightInitializationRule)
            {
                algo = new NoisedWeightInitializationAlgorithm();
            }
            else if (rule is SignChangesRule)
            {
                algo = new SignChangesAlgorithm();
            }
            else if (rule is GradientDescentRule)
            {
                algo = new GradientDescentAlgorithm();
            }
            else if (rule is AlopexBRule)
            {
                algo = new AlopexBAlgorithm();
            }
            else if (rule is GaussianHistoryRule)
            {
                algo = new GaussianHistoryAlgorithm();
            }
            else if (rule is QSARule)
            {
                algo = new QSAAlgorithm();
            }

            if (algo != null) return algo;

            throw new InvalidOperationException("Cannot build Managed Neural Network, because '" + rule.GetType().FullName + "' learning rule type is unknown.");
        }

        #endregion

        #region Iterate

        internal void CallBeforeIterationLearningAlgorithms(NeuralComputationContext context, bool isNewBatch)
        {
            Contract.Requires(context != null);

            lock (context.SyncRoot)
            {
                if (beforeIterationAlgorithms.Length == 0) return;
                foreach (var algo in beforeIterationAlgorithms) algo.ForwardIteration(context, isNewBatch);
            }
        }

        internal void CallErrorBasedBatchLearningAlgorithms(NeuralComputationContext context, int batchSize, IntRange errorBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(OutputInterfaceLength == errorBuffer.Size);
            Contract.Requires(batchSize > 0);

            lock (context.SyncRoot)
            {
                if (errorBasedAlgorithms.Length == 0) return;

                bufferOps.AverageDist(context, avgValueIndex, errorBuffer);

                foreach (var algo in errorBasedAlgorithms) algo.BackwardIteration(context, avgValueIndex, true, batchSize);
            }
        }

        internal void CallErrorBasedStochasticLearningAlgorithms(NeuralComputationContext context, IntRange errorBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(OutputInterfaceLength == errorBuffer.Size);

            lock (context.SyncRoot)
            {
                if (errorBasedAlgorithms.Length == 0) return;

                bufferOps.Average(context, avgValueIndex, errorBuffer);

                foreach (var algo in errorBasedAlgorithms) algo.BackwardIteration(context, avgValueIndex, false);
            }
        }

        #endregion

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

        #region Cleanup

        protected virtual void FreeUnmanagedResources()
        {
        }

        protected virtual void FreeManagedResources()
        {
        }

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
