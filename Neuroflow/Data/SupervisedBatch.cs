using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Data
{
    public sealed class SupervisedBatch : List<SupervisedSample>, IDisposable
    {
        public SupervisedBatch()
        {
        }

        public SupervisedBatch(IEnumerable<SupervisedSample> collection) :
            base(collection)
        {
        }

        public SupervisedBatch(IEnumerable<SupervisedSampleEntry> collection)
        {
            Args.Requires(() => collection, () => collection != null);

            foreach (var item in collection) Add(item);
        }

        public void Add(SupervisedSampleEntry item)
        {
            Add(new SupervisedSample(item));
        }

        public void Add(DataArray input)
        {
            Add(new SupervisedSampleEntry(input));
        }

        public void Add(DataArray input, DataArray desiredOutput, DataArray actualOutput)
        {
            Add(new SupervisedSampleEntry(input, desiredOutput, actualOutput));
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
