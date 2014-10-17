using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural
{
    public sealed class UnitIndexTable
    {
        internal UnitIndexTable(int inputLayerSize, IEnumerable<int> otherLayerSizes)
        {
            Contract.Requires(inputLayerSize > 0);
            Contract.Requires(otherLayerSizes != null);

            InputUnitCount = inputLayerSize;
            OtherLayerSizes = otherLayerSizes.ToArray();
        }
        
        public int InputUnitCount { get; private set; }

        public int[] OtherLayerSizes { get; private set; }
    }
}
