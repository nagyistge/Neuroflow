using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    internal sealed class LayerOrderComparer : IComparer<Layer>
    {
        Dictionary<Tuple<Layer, Layer>, int> results = new Dictionary<Tuple<Layer, Layer>, int>();

        public int Compare(Layer x, Layer y)
        {
            var key = Tuple.Create(x, y);
            int result;

            if (results.TryGetValue(key, out result)) return result;

            var rKey = Tuple.Create(y, x);

            if (results.TryGetValue(rKey, out result)) return -result;

            if (IsBelow(x, y)) result = -1;
            else if (IsBelow(y, x)) result = 1;
            else result = 0;
            results.Add(key, result);
            return result;
        }

        private static bool IsBelow(Layer x, Layer y)
        {
            foreach (var output in x.OutputConnections.GetConnectedLayers(FlowDirection.OneWay | FlowDirection.TwoWay))
            {
                if (output == y) return true;
            }
            foreach (var output in x.OutputConnections.GetConnectedLayers(FlowDirection.OneWay | FlowDirection.TwoWay))
            {
                if (IsBelow(output, y)) return true;
            }
            return false;
        }
    }
}
