using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural
{
    internal sealed class LayerLinkCollection : Collection<ConnectableLayer>
    {
        private const string OwnedByErrorMsg = "Layer is owned by an other definition.";

        internal LayerLinkCollection(ConnectableLayer owner, LinkType linkType)
        {
            Contract.Requires(owner != null);

            OwnerLayer = owner;
            LinkType = linkType;
        }
        
        HashSet<Guid> toLayerUIDs = new HashSet<Guid>();

        internal ConnectableLayer OwnerLayer { get; private set; }

        internal LinkType LinkType { get; private set; }

        bool inConnected, inDisconnected;

        protected override void ClearItems()
        {
            var layers = Count != 0 ? this.ToList() : null;
            base.ClearItems();
            toLayerUIDs.Clear();
            if (layers != null) foreach (var l in layers) Disconnected(l);
        }

        protected override void InsertItem(int index, ConnectableLayer item)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (toLayerUIDs.Contains(item.UID)) return;
            base.InsertItem(index, item);
            toLayerUIDs.Add(item.UID);
            Connected(item);
        }

        protected override void RemoveItem(int index)
        {
            var l = this[index];
            base.RemoveItem(index);
            toLayerUIDs.Remove(l.UID);
            Disconnected(l);
        }

        protected override void SetItem(int index, ConnectableLayer item)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (toLayerUIDs.Contains(item.UID)) return;
            var l = this[index];
            base.SetItem(index, item);
            toLayerUIDs.Add(item.UID);
            toLayerUIDs.Remove(l.UID);
            Connected(item);
            Disconnected(l);
        }

        private void Connected(ConnectableLayer layer)
        {
            if (inConnected) return;

            LayerLinkCollection coll;
            if (LinkType == LinkType.Lower)
            {
                coll = (LayerLinkCollection)layer.UpperLayers;
            }
            else
            {
                coll = (LayerLinkCollection)layer.LowerLayers;
            }

            coll.inConnected = true;
            try
            {
                coll.Add(OwnerLayer);
            }
            finally
            {
                coll.inConnected = false;
            }
        }

        private void Disconnected(ConnectableLayer layer)
        {
            if (inDisconnected) return;

            LayerLinkCollection coll;
            if (LinkType == LinkType.Lower)
            {
                coll = (LayerLinkCollection)layer.UpperLayers;
            }
            else
            {
                coll = (LayerLinkCollection)layer.LowerLayers;
            }

            coll.inDisconnected = true;
            try
            {
                coll.Remove(OwnerLayer);
            }
            finally
            {
                coll.inDisconnected = false;
            }
        }
    }
}
