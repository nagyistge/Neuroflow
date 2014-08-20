using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Computations;
using Neuroflow.Core;
using System.Diagnostics.Contracts;
using Neuroflow.Networks.Neural.Learning;
using System.Collections.ObjectModel;

namespace Neuroflow.Networks.Neural
{
    public abstract class NeuralNetwork : SynchronizedObject, IComputationUnit<float>
    {
        protected NeuralNetwork(ICollection<ConnectableLayer> layers, NNInitParameters initPars)
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
            Built(allocator, cgs, initPars);
        }

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

        protected abstract void Build(BufferAllocator allocator, ConnectedLayerGroups connectedLayerGroups, NNInitParameters initPars);

        protected abstract void Built(BufferAllocator allocator, ConnectedLayerGroups connectedLayerGroups, NNInitParameters initPars);

        protected abstract void InitializeLearningAlgorithms(BufferAllocator allocator, LearningLayerGroups learningLayerGroups, NNInitParameters initPars);

        public void Iteration()
        {
            Iteration(false, null);
        }

        public void Iteration(bool collectTrainingData = false, int? innerIterationIndex = null)
        {
            Contract.Requires(innerIterationIndex == null || (innerIterationIndex.HasValue && RecurrentOptions != null && RecurrentOptions.MaxIterations > innerIterationIndex.Value));
            
            lock (SyncRoot) DoIteration(collectTrainingData, innerIterationIndex);
        }

        public void Backpropagate(BackprogrationMode mode, int? innerIterationIndex = null)
        {
            Contract.Requires(RecurrentOptions == null || (innerIterationIndex.HasValue && RecurrentOptions.MaxIterations > innerIterationIndex.Value));
            Contract.Requires(IsBackwardEnabled);

            lock (SyncRoot) DoBackpropagate(mode, innerIterationIndex);
        }

        public void PropagatePValues(float[] eVector)
        {
            Contract.Requires((StructuralElementFlags & NNStructuralElement.RTLRInformation) != 0);
            Contract.Requires(eVector == null || eVector.Length == OutputInterfaceLength);

            lock (SyncRoot) DoPropagatePValues(eVector);
        }

        public void WriteInput(float[] values)
        {
            Contract.Requires(values != null);
            Contract.Requires(values.Length == InputInterfaceLength);

            lock (SyncRoot) DoWriteInput(values);
        }

        public void ReadOutput(float[] values)
        {
            Contract.Requires(values != null);
            Contract.Requires(values.Length == OutputInterfaceLength);

            lock (SyncRoot) DoReadOutput(values);
        }

        public void WriteError(float[] values)
        {
            Contract.Requires(values != null);
            Contract.Requires(values.Length == OutputInterfaceLength);
            Contract.Requires(IsBackwardEnabled);

            lock (SyncRoot) DoWriteError(values);
        }

        public void Reset()
        {
            Reset(NeuralNetworkResetTarget.All);
        }

        public void Reset(NeuralNetworkResetTarget target)
        {
            lock (SyncRoot)
            {
                if (((target & NeuralNetworkResetTarget.Outputs) != 0) ||
                    ((target & NeuralNetworkResetTarget.Ps) != 0))
                {
                    ResetForwardValues(target);
                }

                if (((target & NeuralNetworkResetTarget.GradientSums) != 0 || 
                     (target & NeuralNetworkResetTarget.Gradients) != 0 || 
                      (target & NeuralNetworkResetTarget.Errors)!= 0) && 
                     IsBackwardEnabled)
                {
                    ResetBackwardValues(target);
                }

                if ((target & NeuralNetworkResetTarget.Algorithms) != 0)
                {
                    ResetAlgorithms();
                }
            }
        }

        unsafe protected abstract internal void CallBeforeIterationLearningAlgorithms(bool isNewBatch);

        unsafe protected abstract internal void CallErrorBasedBatchLearningAlgorithms(int batchSize, double averageError);

        unsafe protected abstract internal void CallErrorBasedStochasticLearningAlgorithms(double averageError);

        unsafe protected abstract void DoIteration(bool collectTrainingData, int? innerIterationIndex);

        unsafe protected abstract void DoBackpropagate(BackprogrationMode mode, int? innerIterationIndex = null);

        unsafe protected abstract void DoPropagatePValues(float[] eVector);

        unsafe protected abstract void DoWriteInput(float[] values);

        unsafe protected abstract void DoReadOutput(float[] values);

        unsafe protected abstract void DoWriteError(float[] values);

        unsafe protected abstract void ResetAlgorithms();

        unsafe protected abstract void ResetForwardValues(NeuralNetworkResetTarget target);

        unsafe protected abstract void ResetBackwardValues(NeuralNetworkResetTarget target);
    }
}
