using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Collections.ObjectModel;

namespace Neuroflow.Networks.Neural
{
    public sealed class ConnectedLayer
    {
        #region Construct

        internal ConnectedLayer(int index, Layer layer, NNStructuralElement structuralElementFlags)
        {
            Contract.Requires(layer != null);
            Contract.Requires(index >= 0);

            Index = index;
            Layer = layer;
            StructuralElementFlags = structuralElementFlags;
        } 

        #endregion

        #region Common Props

        public int Index { get; private set; }

        public Layer Layer { get; private set; }

        public NNStructuralElement StructuralElementFlags { get; private set; }

        public bool IsBiased
        {
            get { return BiasValueIndex != null; }
        }

        public bool IsOutput { get; internal set; }

        #endregion        

        #region Forward Props

        public int? BiasValueIndex { get; internal set; }

        public WeightedValueBuffer[] WeightedInputBuffers { get; internal set; }

        public IntRange OutputBuffer { get; internal set; } 

        #endregion

        #region Gradient Props

        public int? BiasGradientValueIndex { get; internal set; }

        public int? BiasGradientSumValueIndex { get; internal set; }

        public IntRange[] GradientBuffers { get; internal set; }

        public IntRange[] GradientSumBuffers { get; internal set; } 

        #endregion

        #region Backward Props

        public IntRange? ErrorBuffer { get; internal set; }

        public WeightedValueBuffer[] WeightedOutputErrorBuffers { get; internal set; }

        public IntRange? OutputErrorBuffer { get; internal set; }

        #endregion

        #region BPTT Props

        public IntRange[][] InnerItarationInputValueStacks { get; internal set; }

        public IntRange[] InnerItarationOutputValueStack { get; internal set; }

        #endregion

        #region RTLR Props

        public UpperLayerInfo[] UpperNonInputLayerInfos { get; internal set; }

        public IntRange[] PBiasBuffers { get; internal set; }

        public IntRange[][][] PWeightBuffers { get; internal set; }

        public IntRange[] PrevPBiasBuffers { get; internal set; }

        public IntRange[][][] PrevPWeightBuffers { get; internal set; }

        public IntRange? NetDerivBuffer { get; internal set; }

        #endregion
    }
}
