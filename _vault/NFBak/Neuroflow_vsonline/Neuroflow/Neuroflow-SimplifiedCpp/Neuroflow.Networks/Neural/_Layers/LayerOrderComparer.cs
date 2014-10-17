using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Networks.Neural
{
    internal class LayerOrderComparer : Comparer<ConnectableLayer>
    {
        Dictionary<Tuple<Guid, Guid>, int> results = new Dictionary<Tuple<Guid, Guid>, int>();

        public override int Compare(ConnectableLayer x, ConnectableLayer y)
        {
            var key = Tuple.Create(x.UID, y.UID);
            int result;

            if (results.TryGetValue(key, out result)) return result;

            var rKey = Tuple.Create(y.UID, x.UID);

            if (results.TryGetValue(rKey, out result)) return -result;

            if (Below(x, y)) result = -1;
            else if (Below(y, x)) result = 1;
            else result = 0;
            results.Add(key, result);
            return result;
        }

        private static bool Below(ConnectableLayer x, ConnectableLayer y)
        {
            foreach (var lower in x.LowerLayers)
            {
                if (lower == y) return true;
            }
            foreach (var lower in x.LowerLayers)
            {
                if (Below(lower, y)) return true;
            }
            return false;
        }
    }
}
