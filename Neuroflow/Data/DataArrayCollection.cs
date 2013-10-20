using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Data
{
    public class DataArrayCollection : List<DataArray>, IDisposable
    {
        public DataArrayCollection()
        {
        }

        public DataArrayCollection(DataArray item) 
        {
            Args.Requires(() => item, () => item != null);

            Add(item);
        }

        public DataArrayCollection(IEnumerable<DataArray> collection) :
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
