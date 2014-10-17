using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;

namespace Neuroflow.Networks.Neural
{
    public sealed class ConnectableLayer
    {
        public ConnectableLayer(Layer layer)
        {
            Contract.Requires(layer != null);

            Layer = layer;

            UpperLayers = new LayerLinkCollection(this, LinkType.Upper);
            LowerLayers = new LayerLinkCollection(this, LinkType.Lower);
        }

        public Layer Layer { get; private set; }

        public Guid UID
        {
            get { return Layer.UID; }
        }

        public int Size
        {
            get { return Layer.Size; }
        }

        public bool IsBiased
        {
            get { return Layer.IsBiased; }
        }

        internal int Index { get; set; }

        public Collection<ConnectableLayer> UpperLayers { get; private set; }

        public Collection<ConnectableLayer> LowerLayers { get; private set; }

        public override int GetHashCode()
        {
            return UID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj)) return true;
            var layer = obj as Layer;
            return layer != null && layer.UID == UID;
        }
    }
}
