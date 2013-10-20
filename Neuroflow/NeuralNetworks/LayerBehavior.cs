using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace Neuroflow.NeuralNetworks
{
    public abstract class LayerBehavior : IEquatable<LayerBehavior>
    {
        internal class Collection : ICollection<LayerBehavior>
        {
            List<LayerBehavior> behaviors = new List<LayerBehavior>();

            public void Add(LayerBehavior item)
            {
                Args.Requires(() => item, () => item != null);

                if (!behaviors.Any(b => b.GetType() == item.GetType()))
                {
                    behaviors.Add(item);
                }
                else
                {
                    throw new ArgumentException("Behavior of type '" + item.GetType() + "' already present in collection.");
                }
            }

            public bool Remove(LayerBehavior item)
            {
                Args.Requires(() => item, () => item != null);

                return behaviors.Remove(item);
            }

            public void Clear()
            {
                behaviors.Clear();
            }

            public bool Contains(LayerBehavior item)
            {
                return behaviors.Contains(item);
            }

            public void CopyTo(LayerBehavior[] array, int arrayIndex)
            {
                behaviors.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return behaviors.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public IEnumerator<LayerBehavior> GetEnumerator()
            {
                return behaviors.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return behaviors.GetEnumerator();
            }
        }

        public bool Equals(LayerBehavior other)
        {
            return object.ReferenceEquals(this, other) || (!object.ReferenceEquals(other, null) && other.GetType() == GetType() && PropsEquals(other));
        }

        protected abstract bool PropsEquals(LayerBehavior other);

        sealed public override bool Equals(object obj)
        {
            return Equals(obj as LayerBehavior);
        }

        sealed public override int GetHashCode()
        {
            return GenerateHashCode();
        }

        protected abstract int GenerateHashCode();

        public static bool operator==(LayerBehavior b1, LayerBehavior b2)
        {
            if (object.ReferenceEquals(b1, null)) return object.ReferenceEquals(b2, null);
            return b1.Equals(b2);
        }

        public static bool operator!=(LayerBehavior b1, LayerBehavior b2)
        {
            return !(b1 == b2);
        }
    }
}
