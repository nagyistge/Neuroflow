using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;

namespace Neuroflow.Networks.Neural
{
    public abstract class Layer
    {
        protected Layer(int size)
        {
            Contract.Requires(size > 0);

            Size = size;
            UID = Guid.NewGuid();
        }

        public Guid UID { get; private set; }

        public int Size { get; private set; }

        public virtual bool IsBiased
        {
            get { return false; }
        }

        public ConnectableLayer Connectable()
        {
            return new ConnectableLayer(this);
        }

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
