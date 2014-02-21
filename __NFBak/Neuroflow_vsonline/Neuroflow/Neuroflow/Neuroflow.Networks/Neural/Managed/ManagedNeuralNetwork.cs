using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Threading.Tasks;
using Neuroflow.Networks.Neural.Learning;
using System.Diagnostics;
using Neuroflow.Networks.Neural.Learning.Managed;
using Neuroflow.Core.Vectors;

namespace Neuroflow.Networks.Neural.Managed
{
    public sealed class ManagedNeuralNetwork : NeuralNetwork
    {
        #region Construct

        public ManagedNeuralNetwork(ICollection<ConnectableLayer> layers, NNAlgorithm supportedAlgorithms = NNAlgorithm.None)
            : this(layers, new ManagedNNInitParameters(), supportedAlgorithms)
        {
            Contract.Requires(layers != null);
            Contract.Requires(layers.Count != 0);
        }

        public ManagedNeuralNetwork(ICollection<ConnectableLayer> layers, ManagedNNInitParameters parameters, NNAlgorithm supportedAlgorithms = NNAlgorithm.None)
            : base(layers, parameters, supportedAlgorithms)
        {
            Contract.Requires(layers != null);
            Contract.Requires(layers.Count != 0);
            Contract.Requires(parameters != null);
        }

        #endregion

        #region Fields

        int allocatedBuffSize;

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

            BuildForwardComputation(allocator, connectedLayerGroups, (ManagedNNInitParameters)initPars);
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

        private void BuildForwardComputation(BufferAllocator allocator, ConnectedLayerGroups connectedLayerGroups, ManagedNNInitParameters initPars)
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

        private LayerForwardCompute CreateLayerForwardCompute(ConnectedLayer clayer, ManagedNNInitParameters initPars)
        {
            LayerForwardCompute result = null;
            if (clayer.Layer is ActivationLayer)
            {
                result = new ActivationLayerForwardCompute(clayer);
            }
            
            if (result == null) throw new InvalidOperationException("Cannot build Managed Neural Network, because '" + clayer.Layer.GetType().FullName + "' layer type is unknown.");

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

        protected unsafe override NeuralComputationContext DoCreateContext()
        {
            return new ManagedNeuralComputationContext(allocatedBuffSize);
        }

        protected override unsafe Core.Vectors.VectorBuffer<float> DoCreateVectorBuffer()
        {
            return new ManagedVectorBuffer<float>();
        }

        protected override unsafe void DoIteration(NeuralComputationContext context, bool collectTrainingData, int? innerIterationIndex)
        {
            var valueBuffer = GetValueBuff(context);
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

        protected override unsafe void DoBackpropagate(NeuralComputationContext context, BackprogrationMode mode, int? innerIterationIndex = null)
        {
            var valueBuffer = GetValueBuff(context);
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

        protected override unsafe void DoPropagatePValues(NeuralComputationContext context, IntRange? errorBuffer)
        {
            Debug.Assert(pValProp != null);

            var ctx = (ManagedNeuralComputationContext)context;
            var valueBuffer = GetValueBuff(context);
            fixed (float* pValueBuffer = valueBuffer)
            {
                if (errorBuffer == null)
                {
                    pValProp.Propagate(ctx, pValueBuffer, null);
                }
                else
                {
                    pValProp.Propagate(ctx, pValueBuffer, pValueBuffer + errorBuffer.Value.MinValue);
                }
            }
        }

        #endregion

        #region Write and Read

        unsafe protected override void DoWriteInput(NeuralComputationContext context, BufferedVector<float> values)
        {
            int len = values.Length;
            int begin = inputBuffer.MinValue;
            var valueBuffer = GetValueBuff(context);
            fixed (float* pValues = GetValueArray(values), pValueBuffer = valueBuffer)
            {
                for (int i = 0; i < len; i++)
                {
                    pValueBuffer[begin + i] = pValues[i];
                }
            }
        }

        unsafe protected override void DoReadOutput(NeuralComputationContext context, float[] values)
        {
            int len = values.Length;
            int begin = outputBuffer.MinValue;
            var valueBuffer = GetValueBuff(context);
            fixed (float* pValues = values, pValueBuffer = valueBuffer)
            {
                for (int i = 0; i < len; i++)
                {
                    pValues[i] = pValueBuffer[begin + i];
                }
            }
        }

        private static float[] GetValueArray(BufferedVector<float> values)
        {
            var mvb = values.Owner as ManagedVectorBuffer<float>;
            if (mvb == null) throw new InvalidOperationException("Vector source is unknown.");
            return mvb.GetArray(values);
        }

        protected override unsafe void DoComputeError(NeuralComputationContext context, BufferedVector<float> desiredOutputVector, IntRange errorBuffer, IntRange accumulationBuffer)
        {
            var valueBuffer = GetValueBuff(context);
            var mvb = desiredOutputVector.Owner as ManagedVectorBuffer<float>;
            var desiredOutputValues = mvb.GetArray(desiredOutputVector);

            fixed (float* pValueBuffer = valueBuffer, pDesiredOutputValues = desiredOutputValues)
            {
                int outputBegin = outputBuffer.MinValue;
                int errorBegin = errorBuffer.MinValue;
                int accBegin = accumulationBuffer.MinValue;
                int accCount = accumulationBuffer.MaxValue;
                for (int idx = 0; idx < desiredOutputVector.Length; idx++)
                {
                    float desiredValue = pDesiredOutputValues[idx];
                    float currentOutputValue = pValueBuffer[outputBegin + idx];
                    float error = desiredValue - currentOutputValue;
                    pValueBuffer[errorBegin + idx] = error;
                    pValueBuffer[accBegin + idx] += (float)Math.Pow(error * 0.5, 2.0);
                }
                pValueBuffer[accCount]++;
            }
        }

        protected override unsafe void DoSetError(NeuralComputationContext context, IntRange errorBuffer)
        {
            var valueBuffer = GetValueBuff(context);

            fixed (float* pValueBuffer = valueBuffer)
            {
                backwardComputeGroups[0][0].SetErrors(pValueBuffer, errorBuffer);
            }
        }

        protected override unsafe void DoReadError(NeuralComputationContext context, float[] values, IntRange errorBuffer)
        {
            var valueBuffer = GetValueBuff(context);

            fixed (float* pValueBuffer = valueBuffer, pValues = values)
            {
                for (int i = 0; i < values.Length; i++) pValues[i] = pValueBuffer[errorBuffer.MinValue + i];
            }
        }

        protected override unsafe void DoZeroBuffer(NeuralComputationContext context, IntRange accumulationBuffer)
        {
            var valueBuffer = GetValueBuff(context);

            ValueBuffer.Zero(valueBuffer, accumulationBuffer);
        }

        protected override unsafe void DoCalculateAverageError(NeuralComputationContext context, IntRange accumulationBuffer)
        {
            var valueBuffer = GetValueBuff(context);

            fixed (float* pValueBuffer = valueBuffer)
            {
                int accBegin = accumulationBuffer.MinValue;
                float accCount = pValueBuffer[accumulationBuffer.MaxValue];
                if (accCount != 0.0f)
                {
                    int size = accumulationBuffer.Size - 1;
                    for (int idx = 0; idx < size; idx++)
                    {
                        pValueBuffer[accBegin + idx] = pValueBuffer[accBegin + idx] / accCount;
                    }
                }
            }
        }

        protected override unsafe void DoCopyBuffer(NeuralComputationContext context, IntRange source, IntRange target)
        {
            var valueBuffer = GetValueBuff(context);

            fixed (float* pValueBuffer = valueBuffer)
            {
                int size = source.Size;
                for (int i = 0; i < size; i++)
                {
                    pValueBuffer[target.MinValue + i] = pValueBuffer[source.MinValue + i];
                }
            }
        }

        #endregion

        #region Reset

        protected override unsafe void ResetAll(NeuralComputationContext context)
        {
            var valueBuffer = GetValueBuff(context);
            fixed (float* pValueBuffer = valueBuffer)
            {
                Rtl.ZeroMemory((IntPtr)pValueBuffer, (IntPtr)(sizeof(float) * valueBuffer.Length));
            }
        }

        protected override unsafe void ResetAlgorithms(NeuralComputationContext context)
        {
            var valueBuffer = GetValueBuff(context);
            fixed (float* pValueBuffer = valueBuffer) foreach (var algo in algorithms) algo.InitializeNewRun(pValueBuffer);
        }

        protected override unsafe void ResetForwardValues(NeuralComputationContext context, NeuralNetworkResetTarget target)
        {
            var valueBuffer = GetValueBuff(context);

            ValueBuffer.Zero(valueBuffer, inputBuffer);

            for (int groupIndex = 0; groupIndex < forwardComputeGroups.Length; groupIndex++)
            {
                var compute = forwardComputeGroups[groupIndex];
                for (int layerIndex = 0; layerIndex < compute.Length; layerIndex++)
                {
                    compute[layerIndex].Reset(valueBuffer, target);
                }
            }
        }

        protected override unsafe void ResetBackwardValues(NeuralComputationContext context, NeuralNetworkResetTarget target)
        {
            var valueBuffer = GetValueBuff(context);
            for (int groupIndex = 0; groupIndex < backwardComputeGroups.Length; groupIndex++)
            {
                var compute = backwardComputeGroups[groupIndex];
                for (int layerIndex = 0; layerIndex < compute.Length; layerIndex++)
                {
                    compute[layerIndex].Reset(valueBuffer, target);
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
                algo.InitializeAlgo(allocator, group.Rule, group.ConnectedLayers.ToArray(), (ManagedNNInitParameters)initPars);
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

            throw new InvalidOperationException("Cannot build Managed Neural Network, because '" + rule.GetType().FullName + "' learning rule type is unknown.");
        }

        #endregion

        #region Iterate

        protected override unsafe void DoCallBeforeIterationLearningAlgorithms(NeuralComputationContext context, bool isNewBatch)
        {
            if (beforeIterationAlgorithms.Length == 0) return;
            var valueBuffer = GetValueBuff(context);
            fixed (float* pValueBuffer = valueBuffer)
            {
                foreach (var algo in beforeIterationAlgorithms)
                    algo.ForwardIteration(pValueBuffer, isNewBatch);
            }
        }

        protected override unsafe void DoCallErrorBasedBatchLearningAlgorithms(NeuralComputationContext context, int batchSize, IntRange errorBuffer)
        {
            if (errorBasedAlgorithms.Length == 0) return;

            var valueBuffer = GetValueBuff(context);
            fixed (float* pValueBuffer = valueBuffer)
            {
                double averageError = ValueBuffer.AverageDist(pValueBuffer, errorBuffer);

                foreach (var algo in errorBasedAlgorithms)
                    algo.BackwardIteration(pValueBuffer, averageError, true, batchSize);
            }
        }

        protected override unsafe void DoCallErrorBasedStochasticLearningAlgorithms(NeuralComputationContext context, IntRange errorBuffer)
        {
            if (errorBasedAlgorithms.Length == 0) return;

            var valueBuffer = GetValueBuff(context);
            fixed (float* pValueBuffer = valueBuffer)
            {
                double averageError = ValueBuffer.Average(pValueBuffer, errorBuffer);

                foreach (var algo in errorBasedAlgorithms)
                    algo.BackwardIteration(pValueBuffer, averageError, false);
            }
        }

        #endregion

        #region Buff

        private static float[] GetValueBuff(NeuralComputationContext context)
        {
            try
            {
                return ((ManagedNeuralComputationContext)context).Buff;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid context.", ex);
            }
        }

        #endregion

        #endregion
    }
}
