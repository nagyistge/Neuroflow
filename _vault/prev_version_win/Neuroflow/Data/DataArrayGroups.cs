using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Data
{
    public class DataArrayGroups : List<DataArrayCollection>, IDisposable
    {
        public DataArrayGroups()
        {
        }

        public DataArrayGroups(DataArrayCollection item)
        {
            Args.Requires(() => item, () => item != null);

            Add(item);
        }

        public DataArrayGroups(IEnumerable<DataArrayCollection> collection) :
            base(collection)
        {
        }

        bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                foreach (var da in this) da.Dispose();

                GC.SuppressFinalize(this);
            }
        }
    }
}
