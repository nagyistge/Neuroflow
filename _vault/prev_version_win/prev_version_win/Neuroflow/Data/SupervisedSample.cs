using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Data
{
    public sealed class SupervisedSample : List<SupervisedSampleEntry>, IDisposable
    {
        public SupervisedSample()
        {
        }

        public SupervisedSample(IEnumerable<SupervisedSampleEntry> collection) :
            base(collection)
        {
        }

        public SupervisedSample(SupervisedSampleEntry entry)
        {
            Args.Requires(() => entry, () => entry != null);

            Add(entry);
        }

        public SupervisedSample(DataArray input)
        {
            Add(input);
        }

        public SupervisedSample(DataArray input, DataArray desiredOutput, DataArray actualOutput)
        {
            Add(input, desiredOutput, actualOutput);
        }

        public int NumberOfOutputs
        {
            get { return this.Count(i => i.HasOutput); }
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
