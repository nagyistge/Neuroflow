using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Threading.Tasks;
using Neuroflow.Networks.Neural.Learning;
using System.Diagnostics;
using Neuroflow.Networks.Neural.Learning.CPU;

namespace Neuroflow.Networks.Neural.CPU
{
    public sealed class CPUNeuralNetwork : NeuralNetwork
    {
        #region Construct

        public CPUNeuralNetwork(ICollection<ConnectableLayer> layers)
            : this(layers, new CPUNNInitParameters())
        {
            Contract.Requires(layers != null);
            Contract.Requires(layers.Count != 0);
        }

        public CPUNeuralNetwork(ICollection<ConnectableLayer> layers, CPUNNInitParameters parameters)
            : base(layers, parameters)
        {
            Contract.Requires(layers != null);
            Contract.Requires(layers.Count != 0);
            Contract.Requires(parameters != null);
        }

        #endregion

        #region Fields

        float[] valueBuffer;

        IntRange inputBuffer;

        IntRange outputBuffer;

        LayerForwardCompute[][] forwardComputeGroups;

        LayerBackwardCompute[][] backwardComputeGroups;

        LearningAlgorithm[] algorithms, beforeIterationAlgorithms, errorBasedAlgorithms;

        PValuePropagator pValProp;

        #endregion

        #region Build

        #region Entry

        protected override void Build(BufferAllocator allocator, ConnectedLayerGroups connectedLayerGroups, NNInitParameters initPars)
        {
            InitializeInputAndOutput(connectedLayerGroups);

            BuildForwardComputation(allocator, connectedLayerGroups, (CPUNNInitParameters)initPars);
            if (IsBackwardEnabled) BuildBackwardComputation(connectedLayerGroups);
        }

        #endregion

        #region I/O

        private void InitializeInputAndOutput(ConnectedLayerGroups connectedLayerGroups)
        {
            inputBuffer = connectedLayerGroups.InputBuffer;
            outputBuffer = connectedLayerGroups.OutputBuffer;
        } 

        #endregion

        #region Forward

        private void BuildForwardComputation(BufferAllocator allocator, ConnectedLayerGroups connectedLayerGroups, CPUNNInitParameters initPars)
        {
            forwardComputeGroups = new LayerForwardCompute[connectedLayerGroups.Groups.Count][];
            for (int groupIndex = 0; groupIndex < connectedLayerGroups.Groups.Count; groupIndex++)
            {
                var group = connectedLayerGroups.Groups[groupIndex];
                forwardComputeGroups[groupIndex] = new LayerForwardCompute[group.Count];
                for (int layerIndex = 0; layerIndex < group.Count; layerIndex++)
                {
                    forwardComputeGroups[groupIndex][layerIndex] = CreateLayerForwardCompute(group[layerIndex], initPars);
                }
            }
        }

        private LayerForwardCompute CreateLayerForwardCompute(ConnectedLayer clayer, CPUNNInitParameters initPars)
        {
            LayerForwardCompute result = null;
            if (clayer.Layer is ActivationLayer)
            {
                result = new ActivationLayerForwardCompute(clayer);
            }
            
            if (result == null) throw new InvalidOperationException("Cannot build CPU Neural Network, because '" + clayer.Layer.GetType().FullName + "' layer type is unknown.");

            result.RunParallel = initPars.RunParallel;

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

        protected override void Built(BufferAllocator allocator, ConnectedLayerGroups connectedLayerGroups, NNInitParameters initPars)
        {
            // Create buffer:
            valueBuffer = new float[allocator.Size];

            // RTLR:
            if ((StructuralElementFlags & NNStructuralElement.RTLRInformation) != 0)
            {
                pValProp = new PValuePropagator(connectedLayerGroups.IndexTable, forwardComputeGroups.SelectMany(g => g));
            }
        } 

        #endregion

        #endregion

        #region Iteration

        protected override unsafe void DoIteration(bool collectTrainingData, int? innerIterationIndex)
        {
            fixed (float* pValueBuffer = valueBuffer)
            {
                foreach (var group in forwardComputeGroups)
                {
                    for (int layerIndex = 0; layerIndex < group.Length; layerIndex++)
                    {
                        group[layerIndex].Compute(pValueBuffer, collectTrainingData, innerIterationIndex);
                    }
                }
            }
        }

        #endregion

        #region Backpropagate

        protected override unsafe void DoBackpropagate(BackprogrationMode mode, int? innerIterationIndex = null)
        {
            fixed (float* pValueBuffer = valueBuffer)
            {
                foreach (var group in backwardComputeGroups)
                {
                    for (int layerIndex = 0; layerIndex < group.Length; layerIndex++)
                    {
                        group[layerIndex].Compute(pValueBuffer, mode, innerIterationIndex);
                    }
                }
            }
        }

        #endregion

        #region RTLR

        protected override unsafe void DoPropagatePValues(float[] eVector)
        {
            Debug.Assert(pValProp != null);

            fixed (float* pValueBuffer = valueBuffer)
            {
                if (eVector == null)
                {
                    pValProp.Propagate(pValueBuffer, null);
                }
                else
                {
                    fixed (float* e = eVector) pValProp.Propagate(pValueBuffer, e);
                }
            }
        }

        #endregion

        #region Write and Read

        unsafe protected override void DoWriteInput(float[] values)
        {
            int len = values.Length;
            int begin = inputBuffer.MinValue;
            fixed (float* pValues = values, pValueBuffer = valueBuffer)
            {
                for (int i = 0; i < len; i++)
                {
                    pValueBuffer[begin + i] = pValues[i];
                }
            }
        }

        unsafe protected override void DoReadOutput(float[] values)
        {
            int len = values.Length;
            int begin = outputBuffer.MinValue;
            fixed (float* pValues = values, pValueBuffer = valueBuffer)
            {
                for (int i = 0; i < len; i++)
                {
                    pValues[i] = pValueBuffer[begin + i];
                }
            }
        }

        protected override unsafe void DoWriteError(float[] values)
        {
            Debug.Assert(backwardComputeGroups != null);
            Debug.Assert(backwardComputeGroups.Length > 0);
            Debug.Assert(backwardComputeGroups[0].Length == 1);

            fixed (float* pValueBuffer = valueBuffer)
            {
                backwardComputeGroups[0][0].SetErrors(pValueBuffer, values);
            }
        }

        #endregion

        #region Reset

        protected override unsafe void ResetAlgorithms()
        {
            fixed (float* pValueBuffer = valueBuffer) foreach (var algo in algorithms) algo.InitializeNewRun(pValueBuffer);
        }

        protected override unsafe void ResetForwardValues(NeuralNetworkResetTarget target)
        {
            ValueBuffer.Zero(valueBuffer, inputBuffer);

            for (int groupIndex = 0; groupIndex < forwardComputeGroups.Length; groupIndex++)
            {
                var groupCompute = forwardComputeGroups[groupIndex];
                for (int layerIndex = 0; layerIndex < groupCompute.Length; layerIndex++)
                {
                    groupCompute[layerIndex].Reset(valueBuffer, target);
                }
            }
        }

        protected override unsafe void ResetBackwardValues(NeuralNetworkResetTarget target)
        {
            for (int groupIndex = 0; groupIndex < backwardComputeGroups.Length; groupIndex++)
            {
                var groupCompute = backwardComputeGroups[groupIndex];
                for (int layerIndex = 0; layerIndex < groupCompute.Length; layerIndex++)
                {
                    groupCompute[layerIndex].Reset(valueBuffer, target);
                }
            }
        }

        #endregion

        #region Algorithms

        #region Init

        protected override void InitializeLearningAlgorithms(BufferAllocator allocator, LearningLayerGroups learningLayerGroups, NNInitParameters initPars)
        {
            algorithms = new LearningAlgorithm[learningLayerGroups.Count];
            var biAlgos = new LinkedList<LearningAlgorithm>();
            var aeAlgos = new LinkedList<LearningAlgorithm>();

            int idx = 0;
            foreach (var group in learningLayerGroups)
            {
                var algo = CreateAlgorithmForRule(group.Rule);
                algo.InitializeAlgo(allocator, group.Rule, group.ConnectedLayers.ToArray(), (CPUNNInitParameters)initPars);
                algorithms[idx++] = algo;
                if (algo.Rule.IsBeforeIterationRule) biAlgos.AddLast(algo);
                if (algo.Rule.IsErrorBasedRule) aeAlgos.AddLast(algo);
            }

            beforeIterationAlgorithms = biAlgos.ToArray();
            errorBasedAlgorithms = aeAlgos.ToArray();
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

            throw new InvalidOperationException("Cannot build CPU Neural Network, because '" + rule.GetType().FullName + "' learning rule type is unknown.");
        }

        #endregion

        #region Iterate

        protected internal override unsafe void CallBeforeIterationLearningAlgorithms(bool isNewBatch)
        {
            if (beforeIterationAlgorithms.Length == 0) return;
            fixed (float* pValueBuffer = valueBuffer)
            {
                if (beforeIterationAlgorithms.Length > 1)
                {
                    IntPtr ptr = (IntPtr)pValueBuffer;
                    Parallel.ForEach(beforeIterationAlgorithms, (algo) => algo.ForwardIteration((float*)ptr, isNewBatch));
                }
                else
                {
                    beforeIterationAlgorithms[0].ForwardIteration(pValueBuffer, isNewBatch);
                }
            }
        }

        protected internal override unsafe void CallErrorBasedStochasticLearningAlgorithms(double averageError)
        {
            if (errorBasedAlgorithms.Length == 0) return;
            fixed (float* pValueBuffer = valueBuffer)
            {
                if (errorBasedAlgorithms.Length > 1)
                {
                    IntPtr ptr = (IntPtr)pValueBuffer;
                    Parallel.ForEach(errorBasedAlgorithms, (algo) => algo.BackwardIteration((float*)ptr, averageError, false));
                }
                else
                {
                    errorBasedAlgorithms[0].BackwardIteration(pValueBuffer, averageError, false);
                }
            }
        }

        protected internal override unsafe void CallErrorBasedBatchLearningAlgorithms(int batchSize, double averageError)
        {
            if (errorBasedAlgorithms.Length == 0) return; 
            fixed (float* pValueBuffer = valueBuffer)
            {
                if (errorBasedAlgorithms.Length > 1)
                {
                    IntPtr ptr = (IntPtr)pValueBuffer;
                    Parallel.ForEach(errorBasedAlgorithms, (algo) => algo.BackwardIteration((float*)ptr, averageError, true, batchSize));
                }
                else
                {
                    errorBasedAlgorithms[0].BackwardIteration(pValueBuffer, averageError, true, batchSize);
                }
            }
        } 

        #endregion

        #endregion
    }
}
