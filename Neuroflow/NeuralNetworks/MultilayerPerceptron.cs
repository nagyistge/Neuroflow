using Neuroflow.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public class MultilayerPerceptron<T> : MultilayerPerceptron where T : IMultilayerPerceptronAdapter, new()
    {
        public MultilayerPerceptron(IndexedLayerCollection layers, MultilayerPerceptronProperties properties) :
            base(new T(), layers, properties)
        {
        }
    }

    public class MultilayerPerceptron : DisposableObject, IMultilayerPerceptronProperties
    {
        struct LearningInfo
        {
            internal bool Offline { get; set; }

            internal bool Online { get; set; }

            internal LearningAlgoOptimizationType OptimizationType { get; set; }
        }

        class LayerState
        {
            internal class ActivationInfo
            {
                internal ActivationFunction function;
                internal float alpha;
            }

            internal ActivationInfo activationInfo;
        }

        public MultilayerPerceptron(IMultilayerPerceptronAdapter adapter, IndexedLayerCollection layers, MultilayerPerceptronProperties properties)
        {
            Args.Requires(() => properties, () => properties != null);
            Args.Requires(() => adapter, () => adapter != null);
            Args.Requires(() => layers, () => layers != null);

            this.properties = properties;
            this.Adapter = adapter;

            Layers = layers;

            var infos = (from layer in layers
                         from lb in layer.Layer.Behaviors.OfType<LearningBehavior>()
                         let slr = lb as SupervisedLearningRule
                         select new
                         {
                             Index = layer.Index,
                             Info = new LearningInfo
                             {
                                 Offline = slr != null && slr.GetWeightUpdateMode() == WeigthUpdateMode.Offline,
                                 Online = slr != null && slr.GetWeightUpdateMode() == WeigthUpdateMode.Online,
                                 OptimizationType = slr.OptimizationType
                             }
                         }).ToDictionary(o => o.Index, o => o.Info);
            
            isTrainingEnabled = infos.Count > 0;

            isGradientsCalculated = infos.Values.Any(i => i.OptimizationType == LearningAlgoOptimizationType.GradientBased);
            calculateGlobalOfflineError = infos.Values.Any(i => i.OptimizationType == LearningAlgoOptimizationType.Global && i.Offline);
            calculateGlobalOnlineError = calculateGlobalOfflineError || infos.Values.Any(i => i.OptimizationType == LearningAlgoOptimizationType.Global && i.Online);

            doBackpropagate = isTrainingEnabled && isGradientsCalculated && (GradientComputationMethod == GradientComputationMethod.FeedForward || GradientComputationMethod == NeuralNetworks.GradientComputationMethod.BPTT);

            isRecurrent = layers.Any(l => l.Layer.HasRecurrentConnections);

            doFFBP = doBackpropagate && !isRecurrent && GradientComputationMethod == GradientComputationMethod.FeedForward;
            doBPTT = doBackpropagate && isRecurrent && GradientComputationMethod == NeuralNetworks.GradientComputationMethod.BPTT;
            doRTLR = isTrainingEnabled && isRecurrent && GradientComputationMethod == NeuralNetworks.GradientComputationMethod.RTLR;

            if (isRecurrent &&  isGradientsCalculated && !(doBPTT || doRTLR)) throw new InvalidOperationException("Recurrent multilayer perceptron cannot be trained by Feed Forward gradient computation algorithm.");

            CreateStructure(infos);
            CreateCompute();
            CreateTrainInit();
            CreateTrain(infos);
        }

        private MultilayerPerceptronProperties properties;

        public GradientComputationMethod GradientComputationMethod
        {
            get { return properties.GradientComputationMethod; }
        }

        internal IMultilayerPerceptronAdapter Adapter { get; private set; }

        internal IndexedLayerCollection Layers { get; private set; }

        readonly bool isTrainingEnabled;

        readonly bool isGradientsCalculated;

        readonly bool doBackpropagate;

        readonly bool doFFBP;

        readonly bool doBPTT;

        readonly bool doRTLR;

        readonly bool isRecurrent;

        readonly bool calculateGlobalOnlineError;

        readonly bool calculateGlobalOfflineError;

        bool isTrainingInitialized;

        IDeviceArray netInput;

        IDeviceArray netOutput;

        IDeviceArray globalOfflineError;

        IDeviceArray globalOnlineError;

        IDisposable calculateGlobalErrorState;

        RTLR rtlr;

        LinkedList<IDisposable> computationStates = new LinkedList<IDisposable>();

        IDisposable setOutputState;

        Dictionary<int, IDeviceArray> outputs;

        internal Dictionary<int, IDeviceArray> Outputs
        {
            get { return outputs ?? (outputs = new Dictionary<int, IDeviceArray>()); }
        }

        SortedDictionary<int, IDeviceArray> netValueDerivates;

        internal SortedDictionary<int, IDeviceArray> NetValueDerivates
        {
            get { return netValueDerivates ?? (netValueDerivates = new SortedDictionary<int, IDeviceArray>()); }
        }

        Dictionary<int, IDeviceArray> biases;

        internal Dictionary<int, IDeviceArray> Biases
        {
            get { return biases ?? (biases = new Dictionary<int, IDeviceArray>()); }
        }

        Dictionary<int, IDeviceArray> biasGradients;

        internal Dictionary<int, IDeviceArray> BiasGradients
        {
            get { return biasGradients ?? (biasGradients = new Dictionary<int, IDeviceArray>()); }
        }

        Dictionary<int, IDeviceArray> biasGradientSums;

        internal Dictionary<int, IDeviceArray> BiasGradientSums
        {
            get { return biasGradientSums ?? (biasGradientSums = new Dictionary<int, IDeviceArray>()); }
        }

        Dictionary<int, IDeviceArray> errors;

        internal Dictionary<int, IDeviceArray> Errors
        {
            get { return errors ?? (errors = new Dictionary<int, IDeviceArray>()); }
        }

        Dictionary<Tuple<int, int>, IDeviceArray2> weights;

        internal Dictionary<Tuple<int, int>, IDeviceArray2> Weights
        {
            get { return weights ?? (weights = new Dictionary<Tuple<int, int>, IDeviceArray2>()); }
        }

        Dictionary<Tuple<int, int>, IDeviceArray2> gradients;

        internal Dictionary<Tuple<int, int>, IDeviceArray2> Gradients
        {
            get { return gradients ?? (gradients = new Dictionary<Tuple<int, int>, IDeviceArray2>()); }
        }

        Dictionary<Tuple<int, int>, IDeviceArray2> gradientSums;

        internal Dictionary<Tuple<int, int>, IDeviceArray2> GradientSums
        {
            get { return gradientSums ?? (gradientSums = new Dictionary<Tuple<int, int>, IDeviceArray2>()); }
        }

        Dictionary<Tuple<int, int>, DeviceArrayStack> inputValueStacks;

        internal Dictionary<Tuple<int, int>, DeviceArrayStack> InputValueStacks
        {
            get { return inputValueStacks ?? (inputValueStacks = new Dictionary<Tuple<int, int>, DeviceArrayStack>()); }
        }

        Dictionary<int, DeviceArrayStack> outputValueStacks;

        internal Dictionary<int, DeviceArrayStack> OutputValueStacks
        {
            get { return outputValueStacks ?? (outputValueStacks = new Dictionary<int, DeviceArrayStack>()); }
        }

        List<Action> computeCode;

        List<Action> ComputeCode
        {
            get { return computeCode ?? (computeCode = new List<Action>()); }
        }

        List<Action<int, bool>> bpttComputeCode;

        List<Action<int, bool>> BPTTComputeCode
        {
            get { return bpttComputeCode ?? (bpttComputeCode = new List<Action<int, bool>>()); }
        }

        List<Action> trainInitCode;

        List<Action> TrainInitCode
        {
            get { return trainInitCode ?? (trainInitCode = new List<Action>()); }
        }

        List<Action> ffBackpropagateCode;

        List<Action> FFBackpropagateCode
        {
            get { return ffBackpropagateCode ?? (ffBackpropagateCode = new List<Action>()); }
        }

        List<Action<int, bool, int?>> bpttBackpropagateCode;

        List<Action<int, bool, int?>> BPTTBackpropagateCode
        {
            get { return bpttBackpropagateCode ?? (bpttBackpropagateCode = new List<Action<int, bool, int?>>()); }
        }

        List<ILearningAlgo> onlineSupervisedLearningAlgos;

        List<ILearningAlgo> OnlineSupervisedLearningAlgos
        {
            get { return onlineSupervisedLearningAlgos ?? (onlineSupervisedLearningAlgos = new List<ILearningAlgo>()); }
        }

        List<ILearningAlgo> offlineSuprevisedLearningAlgos;

        List<ILearningAlgo> OfflineSupervisedLearningAlgos
        {
            get { return offlineSuprevisedLearningAlgos ?? (offlineSuprevisedLearningAlgos = new List<ILearningAlgo>()); }
        }

        LayerState[] layerStates;

        LayerState[] LayerStates
        {
            get { return layerStates ?? (layerStates = Enumerable.Range(0, Layers.Count).Select(i => new LayerState()).ToArray()); }
        }

        LinkedList<IDisposable> marshaledInstances;

        LinkedList<IDisposable> MarshaledInstances
        {
            get { return marshaledInstances ?? (marshaledInstances = new LinkedList<IDisposable>()); }
        }

        public int InputSize
        {
            get { return Layers[0].Layer.Size; }
        }

        public int OutputSize
        {
            get { return Layers[Layers.Count - 1].Layer.Size; }
        }

        public int NumberOfWeights
        {
            get { return Weights.Values.Select(a => a.Size).Sum() + Biases.Values.Select(a => a.Size).Sum(); }
        }

        #region Interface

        public void GetWeights(DataArray values)
        {
            Args.Requires(() => values, () => values != null);

            DoGetWeights(values);
        }

        public void SetWeights(DataArray values)
        {
            Args.Requires(() => values, () => values != null);

            DoSetWeights(values);
        }

        public void Compute(DataArray inputs, DataArray outputs)
        {
            Compute(new DataArrayCollection(inputs), new DataArrayCollection(outputs));
        }

        public void Train(DataArray input, DataArray desiredOutputs, DataArray actualOutputs)
        {
            Train(new SupervisedSample(input, desiredOutputs, actualOutputs));
        }

        public void Train(SupervisedSampleEntry sampleEntry)
        {
            Args.Requires(() => sampleEntry, () => sampleEntry != null && sampleEntry.HasOutput);

            Train(new SupervisedSample(sampleEntry));
        }

        public void Train(SupervisedSample sample)
        {
            Train(new SupervisedBatch(sample));
        }

        public void Compute(DataArrayCollection inputs, DataArrayCollection outputs)
        {
            Args.Requires(() => inputs, () => inputs != null);
            Args.Requires(() => outputs, () => outputs != null);
            Args.Requires(() => inputs, () => inputs.Count > 0);
            Args.Requires(() => inputs, () => inputs.Count == outputs.Count);

            DoCompute(inputs, outputs);
        }

        public void Train(SupervisedBatch batch)
        {
            Args.Requires(() => batch, () => batch != null);
            Args.Requires(() => batch, () => batch.Count > 0);

            DoTrain(batch);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (computationStates != null)
            {
                ResourceManager.Free(computationStates);
                computationStates = null;
            }
            if (outputs != null)
            {
                ResourceManager.Free(outputs.Values);
                outputs = null;
            }
            if (biases != null)
            {
                ResourceManager.Free(biases.Values);
                biases = null;
            }
            if (biasGradients != null)
            {
                ResourceManager.Free(biasGradients.Values);
                biasGradients = null;
            }
            if (biasGradientSums != null)
            {
                ResourceManager.Free(biasGradientSums.Values);
                biasGradientSums = null;
            }
            if (errors != null)
            {
                ResourceManager.Free(errors.Values);
                errors = null;
            }
            if (weights != null)
            {
                ResourceManager.Free(weights.Values);
                weights = null;
            }
            if (gradients != null)
            {
                ResourceManager.Free(gradients.Values);
                gradients = null;
            }
            if (gradientSums != null)
            {
                ResourceManager.Free(gradientSums.Values);
                gradientSums = null;
            }
            if (onlineSupervisedLearningAlgos != null)
            {
                ResourceManager.Free(onlineSupervisedLearningAlgos);
                onlineSupervisedLearningAlgos = null;
            }
            if (offlineSuprevisedLearningAlgos != null)
            {
                ResourceManager.Free(offlineSuprevisedLearningAlgos);
                offlineSuprevisedLearningAlgos = null;
            }
            if (inputValueStacks != null)
            {
                ResourceManager.Free(inputValueStacks.Values);
                inputValueStacks = null;
            }
            if (outputValueStacks != null)
            {
                ResourceManager.Free(outputValueStacks.Values);
                outputValueStacks = null;
            }
            if (marshaledInstances != null)
            {
                ResourceManager.Free(marshaledInstances);
                marshaledInstances = null;
            }
            if (globalOfflineError != null)
            {
                ResourceManager.Free(globalOfflineError);
                globalOfflineError = null;
            }
            if (globalOnlineError != null)
            {
                ResourceManager.Free(globalOnlineError);
                globalOnlineError = null;
            }
            if (calculateGlobalErrorState != null)
            {
                ResourceManager.Free(calculateGlobalErrorState);
                calculateGlobalErrorState = null;
            }
        }

        private void CreateStructure(Dictionary<int, LearningInfo> learningInfos)
        {
            var daMan = Adapter.DeviceArrayManagement;
            var comp = Adapter.ComputeActivation;

            for (int lidx = 0; lidx < Layers.Count; lidx++)
            {
                var learningInfo = GetLearningInfo(lidx, learningInfos);
                bool isInput = lidx == 0;
                bool isOutput = lidx == Layers.Count - 1;

                var layer = Layers[lidx];

                if (!isInput)
                {
                    // Output:
                    if (!isOutput)
                    {
                        var outputArray = daMan.CreateArray(false, layer.Layer.Size);
                        Outputs.Add(lidx, outputArray);
                    }

                    // Net Value Derivates:
                    if (doRTLR)
                    {
                        var netVDArray = daMan.CreateArray(false, layer.Layer.Size);
                        NetValueDerivates.Add(lidx, netVDArray);
                    }

                    // Bias:
                    Biases.Add(lidx, daMan.CreateArray(true, layer.Layer.Size));

                    // For gradients:
                    if (doBackpropagate)
                    {
                        // Errors:
                        Errors.Add(lidx, daMan.CreateArray(false, layer.Layer.Size));
                    }

                    if (learningInfo.Online || doBPTT)
                    {
                        // Bias Gradients:
                        BiasGradients.Add(lidx, daMan.CreateArray(false, layer.Layer.Size));
                    }

                    if (learningInfo.Offline)
                    {
                        // Bias Gradient Sums:
                        BiasGradientSums.Add(lidx, daMan.CreateArray(false, layer.Layer.Size));
                    }

                    for (int iidx = 0; iidx < layer.Layer.GetInputLayers().Count(); iidx++)
                    {
                        var inputLayer = layer.Layer.GetInputLayer(iidx);
                        var key = Tuple.Create(GetLayerIndex(inputLayer), lidx);

                        // Weights
                        Weights.Add(key, daMan.CreateArray2(true, inputLayer.Size, layer.Layer.Size));

                        if (learningInfo.Online || doBPTT)
                        {
                            // Gradients:
                            Gradients.Add(key, daMan.CreateArray2(true, inputLayer.Size, layer.Layer.Size));
                        }

                        if (learningInfo.Offline)
                        {
                            // Gradient Sums:
                            GradientSums.Add(key, daMan.CreateArray2(true, inputLayer.Size, layer.Layer.Size));
                        }

                        if (doBPTT)
                        {
                            InputValueStacks[key] = new DeviceArrayStack(Adapter.DeviceArrayManagement, inputLayer.Size);
                        }
                    }

                    if (doBPTT)
                    {
                        OutputValueStacks[lidx] = new DeviceArrayStack(Adapter.DeviceArrayManagement, layer.Layer.Size);
                    }
                }
            }
        }

        private static LearningInfo GetLearningInfo(int lidx, Dictionary<int, LearningInfo> learningInfos)
        {
            LearningInfo result;
            learningInfos.TryGetValue(lidx, out result);
            return result;
        }

        internal int GetLayerIndex(Layer layer)
        {
            return (from l in Layers
                    where l.Layer == layer
                    select l.Index).Single();
        }

        private void CreateCompute()
        {
            for (int i = 1; i < Layers.Count; i++)
            {
                int lidx = i;
                var layer = Layers[lidx];

                var inputsList = new List<DeviceArrayFactory>();
                var bpttInputKeys = doBPTT ? new List<Tuple<int, int>>() : null;

                var weightsList = new List<IDeviceArray2>();

                foreach (var inputConnectedLayer in layer.Layer.GetInputLayers())
                {
                    int inputIndex = GetLayerIndex(inputConnectedLayer);
                    var key = Tuple.Create(inputIndex, lidx);

                    // Inputs:
                    DeviceArrayFactory inputConnectedOutputArray = () => GetNetValues(inputIndex);
                    inputsList.Add(inputConnectedOutputArray);

                    if (doBPTT)
                    {
                        Debug.Assert(bpttInputKeys != null);
                        bpttInputKeys.Add(key);
                    }

                    // Weights:
                    var w = Weights[key];
                    weightsList.Add(w);
                }

                // Activation func
                ActivationFunction function;
                float alpha;
                GetActivationData(layer, out function, out alpha);

                // Bias
                var currentBias = Biases[lidx];

                // Output
                DeviceArrayFactory currentOutputs = () => GetNetValues(lidx);

                // Net V Deriv
                IDeviceArray netValueDerivates = doRTLR ? NetValueDerivates[lidx] : null;

                var inputsA = AsMarshaled(inputsList.ToArray());
                var bpttInputKeysA = AsMarshaled(bpttInputKeys != null ? bpttInputKeys.ToArray() : null);
                var weightsA = AsMarshaled(weightsList.ToArray());

                var fComps = Adapter.ComputeActivation;

                var state = fComps.CreateComputationState();
                computationStates.AddLast(state);

                // Create code:

                if (doBPTT)
                {
                    BPTTComputeCode.Add(
                        (innerIterationIndex, isLastIteration) =>
                        {
                            var outputs = currentOutputs();

                            fComps.ComputeForward(
                                state,
                                inputsA,
                                weightsA,
                                currentBias,
                                currentOutputs(),
                                function,
                                alpha,
                                lidx != 1,
                                lidx != Layers.Count - 1);

                            if (!isLastIteration)
                            {
                                for (int inputIndex = 0; inputIndex < inputsA.ManagedObject.Length; inputIndex++)
                                {
                                    var key = bpttInputKeysA.ManagedObject[inputIndex];
                                    InputValueStacks[key].Push(innerIterationIndex, inputsA.ManagedObject[inputIndex]());
                                }

                                OutputValueStacks[lidx].Push(innerIterationIndex, outputs);
                            }
                        });
                }
                else if (doRTLR)
                {
                    ComputeCode.Add(
                        () =>
                        {
                            fComps.ComputeForwardRTLR(
                                state,
                                inputsA,
                                weightsA,
                                currentBias,
                                currentOutputs(),
                                netValueDerivates,
                                function,
                                alpha,
                                lidx != 1,
                                lidx != Layers.Count - 1);
                        });
                }
                else
                {
                    ComputeCode.Add(
                        () =>
                        {
                            fComps.ComputeForward(
                                state,
                                inputsA,
                                weightsA,
                                currentBias,
                                currentOutputs(),
                                function,
                                alpha,
                                lidx != 1,
                                lidx != Layers.Count - 1);
                        });
                }
            }
        }

        internal Marshaled<T> AsMarshaled<T>(T obj) where T : class
        {
            var m = new Marshaled<T>(obj);
            MarshaledInstances.AddLast(m);
            return m;
        }

        private void CreateTrainInit()
        {
            foreach (var layer in Layers)
            {
                foreach (var beh in layer.Layer.Behaviors)
                {
                    // UniformRandomizeWeights:
                    var uniRandWs = beh as UniformRandomizeWeights;
                    if (uniRandWs != null)
                    {
                        var utils = Adapter.VectorUtils;
                        TrainInitCode.Add(
                            () =>
                            {
                                foreach (var b in Biases.Values) utils.RandomizeUniform(b, -uniRandWs.Strength, uniRandWs.Strength);
                                foreach (var w in Weights.Values) utils.RandomizeUniform(w, -uniRandWs.Strength, uniRandWs.Strength);
                            });
                    }
                }
            }
        }

        private void CreateTrain(Dictionary<int, LearningInfo> learningInfos)
        {
            if (doBackpropagate)
            {
                // Function globals:
                bool gIsLastIteration = false;
                int gInnerIterationIndex = -1;

                for (int i = Layers.Count - 1; i >= 1; i--)
                {
                    int lidx = i;
                    var layer = Layers[lidx];
                    var learningInfo = GetLearningInfo(lidx, learningInfos);

                    if (!learningInfo.Online && !learningInfo.Offline) continue;

                    var inputs = new List<DeviceArrayFactory>();
                    
                    List<IDeviceArray2> gradients = null;
                    if (learningInfo.Online || doBPTT) gradients = new List<IDeviceArray2>();
                    
                    List<IDeviceArray2> gradientSums = null;
                    if (learningInfo.Offline) gradientSums = new List<IDeviceArray2>();
                    
                    var lowerWeights = new List<IDeviceArray2>();
                    
                    var lowerErrors = new List<IDeviceArray>();

                    foreach (var inputConnectedLayer in layer.Layer.GetInputLayers())
                    {
                        int inputIndex = GetLayerIndex(inputConnectedLayer);
                        var key = Tuple.Create(inputIndex, lidx);

                        // Inputs:
                        if (doFFBP)
                        {
                            DeviceArrayFactory inputConnectedOutputArray = () => GetNetValues(inputIndex);
                            inputs.Add(inputConnectedOutputArray);
                        }
                        else if (doBPTT)
                        {
                            DeviceArrayFactory inputConnectedOutputArray = () =>
                            {
                                if (gIsLastIteration)
                                {
                                    return GetNetValues(inputIndex);
                                }
                                else
                                {
                                    return InputValueStacks[key][gInnerIterationIndex];
                                }
                            };
                            inputs.Add(inputConnectedOutputArray);
                        }

                        // Gradients:
                        if (learningInfo.Online || doBPTT) gradients.Add(Gradients[key]);

                        // Gradient Sums:
                        if (learningInfo.Offline) gradientSums.Add(GradientSums[key]);
                    }

                    foreach (var outputConnectedLayer in layer.Layer.GetOutputLayers())
                    {
                        int outputIndex = GetLayerIndex(outputConnectedLayer);

                        // Lower Errors:
                        lowerErrors.Add(Errors[outputIndex]);

                        // Lower Weights:
                        var key = Tuple.Create(lidx, outputIndex);

                        lowerWeights.Add(Weights[key]);
                    }

                    // Activation func
                    ActivationFunction function;
                    float alpha;
                    GetActivationData(layer, out function, out alpha);

                    // Output
                    DeviceArrayFactory outputs = null;
                    if (doFFBP) outputs = () => GetNetValues(lidx);
                    if (doBPTT) outputs = 
                        () =>
                        {
                            if (gIsLastIteration)
                            {
                                return GetNetValues(lidx);
                            }
                            else
                            {
                                return OutputValueStacks[lidx][gInnerIterationIndex];
                            }
                        };

                    // Error
                    var currentErrors = Errors[lidx];

                    // Bias Gradients
                    IDeviceArray biasGradients = null;
                    if (learningInfo.Online || doBPTT) biasGradients = BiasGradients[lidx];

                    // Bias Gradient Sums
                    IDeviceArray biasGradientSums = null;
                    if (learningInfo.Offline) biasGradientSums = BiasGradientSums[lidx];

                    var inputsA = AsMarshaled(inputs.ToArray());
                    var gradientsA = AsMarshaled(gradients != null ? gradients.ToArray() : null);
                    var gradientSumsA = AsMarshaled(gradientSums != null ? gradientSums.ToArray() : null);
                    var lowerWeightsA = AsMarshaled(lowerWeights.ToArray());
                    var lowerErrorsA = AsMarshaled(lowerErrors.ToArray());

                    var comp = Adapter.ComputeActivation;

                    if (lidx == Layers.Count - 1)
                    {
                        // Errors are computed:
                        if (doFFBP)
                        {
                            var state = comp.CreateComputationState();
                            computationStates.AddLast(state);

                            FFBackpropagateCode.Add(
                                () =>
                                {
                                    comp.ComputeGradientsFF(
                                        state,
                                        inputsA,
                                        gradientsA,
                                        biasGradients,
                                        gradientSumsA,
                                        biasGradientSums,
                                        currentErrors,
                                        lidx != 1);
                                });
                        }
                        else if (doBPTT)
                        {
                            var state1 = comp.CreateComputationState();
                            computationStates.AddLast(state1);
                            var state2 = comp.CreateComputationState();
                            computationStates.AddLast(state2);

                            BPTTBackpropagateCode.Add(
                                (innerIterationIndex, isLastIteration, innerIterationsCount) =>
                                {
                                    gIsLastIteration = isLastIteration;
                                    gInnerIterationIndex = innerIterationIndex;

                                    if (innerIterationsCount.HasValue)
                                    {
                                        comp.ComputeGradientsBPTTPhase2(
                                            state2,
                                            inputsA,
                                            gradientsA,
                                            biasGradients,
                                            gradientSumsA,
                                            biasGradientSums,
                                            currentErrors,
                                            lidx != 1,
                                            innerIterationsCount.Value);
                                    }
                                    else
                                    {
                                        comp.ComputeGradientsBPTTPhase1(
                                            state1,
                                            inputsA,
                                            gradientsA,
                                            biasGradients,
                                            currentErrors,
                                            lidx != 1);
                                    }
                                });
                        }
                        else if (doRTLR)
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        if (doFFBP)
                        {
                            var state1 = comp.CreateComputationState();
                            var state2 = comp.CreateComputationState();
                            computationStates.AddLast(state1);
                            computationStates.AddLast(state2);

                            FFBackpropagateCode.Add(
                                () =>
                                {
                                    comp.ComputeErrors(
                                        state1,
                                        outputs(),
                                        currentErrors,
                                        lowerWeightsA,
                                        lowerErrorsA,
                                        function,
                                        alpha);

                                    comp.ComputeGradientsFF(
                                        state2,
                                        inputsA,
                                        gradientsA,
                                        biasGradients,
                                        gradientSumsA,
                                        biasGradientSums,
                                        currentErrors,
                                        lidx != 1);
                                });
                        }
                        else if (doBPTT)
                        {
                            var state1 = comp.CreateComputationState();
                            computationStates.AddLast(state1);
                            var state2 = comp.CreateComputationState();
                            computationStates.AddLast(state2);
                            var state3 = comp.CreateComputationState();
                            computationStates.AddLast(state3);

                            BPTTBackpropagateCode.Add(
                                (innerIterationIndex, isLastIteration, innerIterationsCount) =>
                                {
                                    gIsLastIteration = isLastIteration;
                                    gInnerIterationIndex = innerIterationIndex;

                                    comp.ComputeErrors(
                                        state1,
                                        outputs(),
                                        currentErrors,
                                        lowerWeightsA,
                                        lowerErrorsA,
                                        function,
                                        alpha);

                                    if (innerIterationsCount.HasValue)
                                    {
                                        comp.ComputeGradientsBPTTPhase2(
                                            state2,
                                            inputsA,
                                            gradientsA,
                                            biasGradients,
                                            gradientSumsA,
                                            biasGradientSums,
                                            currentErrors,
                                            lidx != 1,
                                            innerIterationsCount.Value);
                                    }
                                    else
                                    {
                                        comp.ComputeGradientsBPTTPhase1(
                                            state3,
                                            inputsA,
                                            gradientsA,
                                            biasGradients,
                                            currentErrors,
                                            lidx != 1);
                                    }
                                });
                        }                    
                    }
                }
            }
            else if (doRTLR)
            {
                rtlr = new RTLR(this);
            }

            if (calculateGlobalOnlineError || calculateGlobalOfflineError)
            {
                if (calculateGlobalOnlineError) globalOnlineError = Adapter.DeviceArrayManagement.CreateArray(false, OutputSize);
                if (calculateGlobalOfflineError) globalOfflineError = Adapter.DeviceArrayManagement.CreateArray(false, OutputSize);
                calculateGlobalErrorState = Adapter.ComputeActivation.CreateComputationState();
            }

            // Initialize Learning:
            InitializeLearningAlgos();
        }

        void DoCompute(DataArrayCollection inputs, DataArrayCollection outputs)
        {
            for (int i = 0; i < inputs.Count; i++) ComputeSampleEntry(inputs[i], outputs[i]);
        }

        private void ComputeSampleEntry(IDeviceArray inputs, IDeviceArray outputs)
        {
            SetupNetInputAndOutput(inputs, outputs);

            foreach (var c in ComputeCode) c();
        }

        private void BPTTComputeSampleEntry(IDeviceArray inputs, IDeviceArray outputs, int innerIterationIndex, bool isLastIteration)
        {
            Debug.Assert(doBPTT);

            SetupNetInputAndOutput(inputs, outputs);

            foreach (var c in BPTTComputeCode) c(innerIterationIndex, isLastIteration);
        }

        private void RTLRComputeSampleEntry(IDeviceArray inputs, IDeviceArray outputs, DataArray desiredOutputs)
        {
            ComputeSampleEntry(inputs, outputs);

            rtlr.ComputeGradients(desiredOutputs);
        }

        void DoTrain(SupervisedBatch batch)
        {
            // Ensure initialized:
            if (!isTrainingInitialized)
            {
                foreach (var codeEntry in TrainInitCode) codeEntry();
                foreach (var onlineAlgo in OnlineSupervisedLearningAlgos) onlineAlgo.Initialize();
                foreach (var offlineAlgo in OfflineSupervisedLearningAlgos) offlineAlgo.Initialize();

                isTrainingInitialized = true;
            }

            if (doBackpropagate)
            {
                if (doFFBP)
                {
                    FeedForwardTraining(batch);
                }
                else
                {
                    BPTTTraining(batch);
                }
            }
            else if (doRTLR)
            {
                RTLRTraining(batch);
            }
            else
            {
                GlobalOptimizationTraining(batch);
            }
        }

        private void GlobalOptimizationTraining(SupervisedBatch batch)
        {
            Debug.Assert(!isGradientsCalculated);

            // Global Optimization
            for (int sampleIndex = 0, innerIterationCount = 0; sampleIndex < batch.Count; sampleIndex++)
            {
                var sample = batch[sampleIndex];

                int lastEntryIndex = sample.Count - 1;
                bool isLastSample = sampleIndex == batch.Count - 1;

                // Compute forward + error:

                DataArray actualOutputs = null;
                for (int entryIndex = 0; entryIndex < sample.Count; entryIndex++)
                {
                    var entry = sample[entryIndex];

                    if (actualOutputs == null) actualOutputs = FindActualOutput(sample, entryIndex);

                    ComputeSampleEntry(entry.Input, actualOutputs);

                    if (entry.DesiredOutput != null)
                    {
                        CalculateGlobalError(entry.DesiredOutput, actualOutputs);
                        OnlineSupervisedLearning();
                        innerIterationCount++;
                    }

                    if (actualOutputs == entry.ActualOutput) actualOutputs = null;
                }

                if (isLastSample)
                {
                    // Do batch algos
                    OfflineSupervisedLearning(innerIterationCount);
                    ResetGlobalOfflineError();
                }
                if (sample.Count > 1) ResetOutputs();
            }
        }

        private void RTLRTraining(SupervisedBatch batch)
        {
            Debug.Assert(doRTLR);
            Debug.Assert(isGradientsCalculated);

            for (int sampleIndex = 0, innerIterationCount = 0; sampleIndex < batch.Count; sampleIndex++)
            {
                var sample = batch[sampleIndex];
                if (sample.Count <= 1) throw CreateRecSampleExpectedEx();

                int lastEntryIndex = sample.Count - 1;
                bool isLastSample = sampleIndex == batch.Count - 1;

                // Compute forward + gradients:
                DataArray actualOutputs = null;
                for (int entryIndex = 0; entryIndex < sample.Count; entryIndex++)
                {
                    var entry = sample[entryIndex];

                    if (actualOutputs == null) actualOutputs = FindActualOutput(sample, entryIndex);

                    RTLRComputeSampleEntry(entry.Input, actualOutputs, entry.DesiredOutput);

                    if (entry.DesiredOutput != null)
                    {
                        CalculateGlobalError(entry.DesiredOutput, actualOutputs);
                        OnlineSupervisedLearning();
                        innerIterationCount++;
                    }

                    if (actualOutputs == entry.ActualOutput) actualOutputs = null;
                }

                if (isLastSample)
                {
                    // Do batch algos
                    OfflineSupervisedLearning(innerIterationCount);
                    ResetGradientSums();
                    ResetGlobalOfflineError();
                }
                ResetOutputs();
                rtlr.Reset();
            }
        }

        private void BPTTTraining(SupervisedBatch batch)
        {
            Debug.Assert(isGradientsCalculated);
            Debug.Assert(doBPTT);

            // Backpropagation Through Time:

            for (int sampleIndex = 0; sampleIndex < batch.Count; sampleIndex++)
            {
                var sample = batch[sampleIndex];
                if (sample.Count <= 1) throw CreateRecSampleExpectedEx();

                int lastEntryIndex = sample.Count - 1;
                bool isLastSample = sampleIndex == batch.Count - 1;
                int innerIteartionIndex = 0;

                // Compute forward:
                DataArray actualOutputs = null;
                for (int entryIndex = 0; entryIndex < sample.Count; entryIndex++)
                {
                    var entry = sample[entryIndex];

                    if (actualOutputs == null) actualOutputs = FindActualOutput(sample, entryIndex);

                    BPTTComputeSampleEntry(entry.Input, actualOutputs, entryIndex, entryIndex == lastEntryIndex);

                    if (actualOutputs == entry.ActualOutput) actualOutputs = null;
                }

                bool isErrorsZeroed = true;
                for (int entryIndex = lastEntryIndex, ii = 0; entryIndex >= 0; entryIndex--, ii++)
                {
                    var entry = sample[entryIndex];
                    bool isLast = entryIndex == lastEntryIndex;
                    int? innerIterationsCount = entryIndex == 0 ? sample.Count : (int?)null;

                    // Setup output error:
                    SetOutputErrorBPTT(entry.DesiredOutput, ii, isLast, ref isErrorsZeroed);

                    if (entry.DesiredOutput != null)
                    {
                        CalculateGlobalError(entry.DesiredOutput, actualOutputs);
                        innerIteartionIndex++;
                    }

                    // Backpropagate:
                    foreach (var codeEntry in BPTTBackpropagateCode) codeEntry(entryIndex, isLast, innerIterationsCount);
                }

                // Do Gradient based online algo step
                OnlineSupervisedLearning();
                if (isLastSample)
                {
                    // Do batch algos
                    OfflineSupervisedLearning(innerIteartionIndex);
                    ResetGradientSums();
                    ResetGlobalOfflineError();
                }
                ResetGradients();
                ResetOutputs();
                ResetErrors();
            }
        }

        private static InvalidOperationException CreateRecSampleExpectedEx()
        {
            return new InvalidOperationException("Recurrent networks cannot be trained by using feed forward samples.");
        }

        private void FeedForwardTraining(SupervisedBatch batch)
        {
            Debug.Assert(isGradientsCalculated);

            // Feed Forward:

            // Start batch:
            foreach (var sample in batch)
            {
                var entry = sample[0];

                // Compute forward:
                ComputeSampleEntry(entry.Input, entry.ActualOutput);

                // Setup output error:
                SetOutputErrorFF(entry.DesiredOutput);

                // Backpropagate:
                foreach (var codeEntry in FFBackpropagateCode) codeEntry();

                // Do Gradient based online algo step
                OnlineSupervisedLearning();
            }


            // Do batch algos
            OfflineSupervisedLearning(batch.Count);
            ResetGradientSums();
            ResetGlobalOfflineError();
        }

        private DataArray FindActualOutput(SupervisedSample sample, int entryIndex)
        {
            if (entryIndex >= sample.Count) throw new InvalidOperationException("Actual output data array is not found in sample.");
            if (entryIndex < 0) return FindActualOutput(sample, 0);
            var entry = sample[entryIndex];
            if (entry.ActualOutput != null) return entry.ActualOutput;
            return FindActualOutput(sample, entryIndex + 1);
        }

        void SetOutputErrorFF(DataArray desiredOutputs)
        {
            int lidx = Layers.Count - 1;

            if (desiredOutputs == null) new InvalidOperationException("Desired output data array is null.");

            var outputLayer = Layers[lidx];
            var outputErrors = Errors[lidx];
            var actualOutputs = GetNetValues(lidx);

            ActivationFunction outputFunction;
            float outputAlpha;
            GetActivationData(outputLayer, out outputFunction, out outputAlpha);

            var comp = Adapter.ComputeActivation;

            if (setOutputState == null)
            {
                setOutputState = comp.CreateComputationState();
                computationStates.AddLast(setOutputState);
            }

            comp.ComputeErrors(
                setOutputState,
                actualOutputs,
                outputErrors,
                desiredOutputs,
                outputFunction,
                outputAlpha);
        }

        void SetOutputErrorBPTT(DataArray desiredOutputs, int innerIterationIndex, bool isLastIteration, ref bool isErrorsZeroed)
        {
            int lidx = Layers.Count - 1;

            if (desiredOutputs == null)
            {
                if (!isErrorsZeroed)
                {
                    Adapter.VectorUtils.Zero(Errors[lidx]);
                    isErrorsZeroed = true;
                }
            }
            else
            {
                var outputLayer = Layers[lidx];
                var outputErrors = Errors[lidx];
                var actualOutputs = isLastIteration ? GetNetValues(lidx) : OutputValueStacks[lidx][innerIterationIndex];

                ActivationFunction outputFunction;
                float outputAlpha;
                GetActivationData(outputLayer, out outputFunction, out outputAlpha);

                var comp = Adapter.ComputeActivation;

                if (setOutputState == null) computationStates.AddLast(setOutputState = comp.CreateComputationState());

                comp.ComputeErrors(
                    setOutputState,
                    actualOutputs,
                    outputErrors,
                    desiredOutputs,
                    outputFunction,
                    outputAlpha);

                isErrorsZeroed = false;
            }
        }

        void CalculateGlobalError(IDeviceArray desiredOutputs, IDeviceArray actualOutputs)
        {
            if (globalOnlineError != null)
            {
                Debug.Assert(calculateGlobalOnlineError);
                Adapter.ComputeActivation.CalculateGlobalError(calculateGlobalErrorState, desiredOutputs, actualOutputs, globalOnlineError, globalOfflineError);
            }
        }

        void DoGetWeights(Data.DataArray values)
        {
            var valuesArray = values as DataArray;
            if (valuesArray == null) throw new InvalidOperationException("Unsupported data array type.");

            int sIdx = 0;
            var man = Adapter.DeviceArrayManagement;

            foreach (var currentBiases in Biases.Values)
            {
                man.Copy(currentBiases, 0, valuesArray, sIdx, currentBiases.Size);

                sIdx += currentBiases.Size;
            }

            foreach (var currentWeights in Weights.Values)
            {
                man.Copy(currentWeights, 0, valuesArray, sIdx, currentWeights.Size);

                sIdx += currentWeights.Size;
            }
        }

        void DoSetWeights(Data.DataArray values)
        {
            var valuesArray = values as DataArray;
            if (valuesArray == null) throw new InvalidOperationException("Unsupported data array type.");

            int sIdx = 0;
            var man = Adapter.DeviceArrayManagement;

            foreach (var currentBiases in Biases.Values)
            {
                man.Copy(valuesArray, sIdx, currentBiases, 0, currentBiases.Size);

                sIdx += currentBiases.Size;
            }

            foreach (var currentWeights in Weights.Values)
            {
                man.Copy(valuesArray, sIdx, currentWeights, 0, currentWeights.Size);

                sIdx += currentWeights.Size;
            }
        }

        private void GetActivationData(IndexedLayer layer, out ActivationFunction function, out float alpha)
        {
            var s = LayerStates[layer.Index];
            if (s.activationInfo != null)
            {
                function = s.activationInfo.function;
                alpha = s.activationInfo.alpha;
            }
            else
            {
                var desc = layer.Layer.Descriptions.OfType<ActivationDescription>().FirstOrDefault();
                if (desc == null) throw new InvalidOperationException("Layer " + layer.Index + " activation description expected.");
                s.activationInfo = new LayerState.ActivationInfo();
                s.activationInfo.function = function = desc.Function;
                s.activationInfo.alpha = alpha = desc.Alpha;
            }
        }

        private void SetupNetInputAndOutput(IDeviceArray input, IDeviceArray output)
        {
            netInput = input;
            netOutput = output;
        }

        internal IDeviceArray GetNetValues(int layerIndex)
        {
            if (layerIndex == 0)
            {
                return netInput;
            }
            else if (layerIndex == Layers.Count - 1)
            {
                return netOutput;
            }
            else
            {
                return Outputs[layerIndex];
            }
        }

        internal IDeviceArray GetBiasGradients(int layerIndex)
        {
            IDeviceArray result = null;
            if (biasGradients != null) biasGradients.TryGetValue(layerIndex, out result);
            return result;
        }

        internal IDeviceArray GetBiasGradientSums(int layerIndex)
        {
            IDeviceArray result = null;
            if (biasGradientSums != null) biasGradientSums.TryGetValue(layerIndex, out result);
            return result;
        }

        internal IDeviceArray2 GetGradients(Tuple<int, int> weightKey)
        {
            IDeviceArray2 result = null;
            if (gradients != null) gradients.TryGetValue(weightKey, out result);
            return result;
        }

        internal IDeviceArray2 GetGradientSums(Tuple<int, int> weightKey)
        {
            IDeviceArray2 result = null;
            if (gradientSums != null) gradientSums.TryGetValue(weightKey, out result);
            return result;
        }

        LinkedList<IDeviceArray> GetWeights(int layerIndex)
        {
            var result = new LinkedList<IDeviceArray>();
            result.AddLast(Biases[layerIndex]);
            foreach (var item in Weights)
            {
                if (item.Key.Item2 == layerIndex) result.AddLast(item.Value);
            }
            return result;
        }

        LinkedList<IDeviceArray> GetGradientsForLearning(int layerIndex)
        {
            if (biasGradients == null || gradients == null) return null;

            var result = new LinkedList<IDeviceArray>();
            result.AddLast(BiasGradients[layerIndex]);
            foreach (var item in Gradients)
            {
                if (item.Key.Item2 == layerIndex) result.AddLast(item.Value);
            }
            return result;
        }

        LinkedList<IDeviceArray> GetGradientSumsForLearning(int layerIndex)
        {
            if (biasGradientSums == null || gradientSums == null) return null;

            var result = new LinkedList<IDeviceArray>();
            result.AddLast(BiasGradientSums[layerIndex]);
            foreach (var item in GradientSums)
            {
                if (item.Key.Item2 == layerIndex) result.AddLast(item.Value);
            }
            return result;
        }

        private void InitializeLearningAlgos()
        {
            var q = from layer in Layers
                    from lb in layer.Layer.Behaviors.OfType<LearningBehavior>()
                    group layer.Index by lb into g
                    select g;

            foreach (var item in q)
            {
                CreateLearningAlgo(item.Key, item);
            }
        }

        private void CreateLearningAlgo(LearningBehavior learningBehavior, IEnumerable<int> layerIndexes)
        {
            var algoFactory = Adapter.LearningAlgoFactory;
            var nodes = from lidx in layerIndexes
                        select new TrainingNode
                        (
                            GetWeights(lidx),
                            GetGradientsForLearning(lidx),
                            GetGradientSumsForLearning(lidx)
                        );

            var algo = algoFactory.CreateLearningAlgo(learningBehavior, nodes.ToList().AsReadOnly());

            if (algo == null) throw new NotSupportedException("Learning behavior of type '" + learningBehavior.GetType().FullName + "' is not supported.");

            if ((algo.IterationTypes & LearningAlgoIterationType.SupervisedOnline) != 0)
            {
                OnlineSupervisedLearningAlgos.Add(algo);
            }

            if ((algo.IterationTypes & LearningAlgoIterationType.SupervisedOffline) != 0)
            {
                OfflineSupervisedLearningAlgos.Add(algo);
            }
        }

        private void OnlineSupervisedLearning()
        {
            foreach (var algo in OnlineSupervisedLearningAlgos) algo.Run(0, globalOnlineError);
        }

        private void OfflineSupervisedLearning(int iterationCount)
        {
            foreach (var algo in OfflineSupervisedLearningAlgos) algo.Run(iterationCount, globalOfflineError);
        }

        #region Reset

        private void ResetGlobalOfflineError()
        {
            if (globalOfflineError != null) Adapter.VectorUtils.Zero(globalOfflineError);
        }

        IDeviceArray[] errorArrays;

        IDeviceArray[] ErrorArrays
        {
            get { return errorArrays ?? (errorArrays = errors != null ? errors.Values.ToArray() : new IDeviceArray[0]); }
        }

        private void ResetErrors()
        {
            Reset(ErrorArrays);
        }

        IDeviceArray[] outputArrays;

        IDeviceArray[] OutputArrays
        {
            get { return outputArrays ?? (outputArrays = outputs != null ? outputs.Values.ToArray() : new IDeviceArray[0]); }
        }

        private void ResetOutputs()
        {
            Reset(OutputArrays);
        }

        IDeviceArray[] gradientArrays;

        IDeviceArray[] GradientArrays
        {
            get
            {
                if (gradientArrays == null)
                {
                    var gs = gradients != null ? gradients.Values : Enumerable.Empty<IDeviceArray>();
                    var bs = biasGradients != null ? biasGradients.Values : Enumerable.Empty<IDeviceArray>();
                    gradientArrays = gs.Concat(bs).ToArray();
                }
                return gradientArrays;
            }
        }

        private void ResetGradients()
        {
            Reset(GradientArrays);
        }

        IDeviceArray[] gradientSumArrays;

        IDeviceArray[] GradientSumArrays
        {
            get
            {
                if (gradientSumArrays == null)
                {
                    var gs = gradientSums != null ? gradientSums.Values : Enumerable.Empty<IDeviceArray>();
                    var bs = biasGradientSums != null ? biasGradientSums.Values : Enumerable.Empty<IDeviceArray>();
                    gradientSumArrays = gs.Concat(bs).ToArray();
                }
                return gradientSumArrays;
            }
        }

        private void ResetGradientSums()
        {
            Reset(GradientSumArrays);
        }

        private void Reset(IDeviceArray[] deviceArrays)
        {
            Debug.Assert(deviceArrays != null);
            foreach (var deviceArray in deviceArrays) Adapter.VectorUtils.Zero(deviceArray);
        }

        #endregion
    }
}
