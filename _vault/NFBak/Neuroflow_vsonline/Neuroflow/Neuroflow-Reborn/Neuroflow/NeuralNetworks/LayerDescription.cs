using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public abstract class LayerDescription
    {
        internal class Collection : ICollection<LayerDescription>
        {
            List<LayerDescription> descriptions = new List<LayerDescription>();

            public void Add(LayerDescription item)
            {
                Args.Requires(() => item, () => item != null);

                if (!descriptions.Any(b => b.GetType() == item.GetType()))
                {
                    descriptions.Add(item);
                }
                else
                {
                    throw new ArgumentException("Description of type '" + item.GetType() + "' already present in collection.");
                }
            }

            public bool Remove(LayerDescription item)
            {
                Args.Requires(() => item, () => item != null);

                return descriptions.Remove(item);
            }

            public void Clear()
            {
                descriptions.Clear();
            }

            public bool Contains(LayerDescription item)
            {
                return descriptions.Contains(item);
            }

            public void CopyTo(LayerDescription[] array, int arrayIndex)
            {
                descriptions.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return descriptions.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public IEnumerator<LayerDescription> GetEnumerator()
            {
                return descriptions.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return descriptions.GetEnumerator();
            }
        }
    }
}
